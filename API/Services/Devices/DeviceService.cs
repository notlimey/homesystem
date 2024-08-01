using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using API.Data;
using API.Interfaces.Devices;
using API.Models.Devices;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Devices;

public class DeviceService : IDeviceService
{
    private readonly HomeDbContext _context;

    public DeviceService(HomeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Device>> GetDevicesAsync()
    {
        return await _context.Devices.ToListAsync();
    }

    public async Task<Device> GetDeviceByIdAsync(Guid id)
    {
        return await _context.Devices.FindAsync(id);
    }

    public async Task<Device> CreateDeviceAsync(Device device)
    {
        device.Id = Guid.NewGuid();
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<Device> UpdateDeviceAsync(Guid id, Device device)
    {
        if (id != device.Id)
            return null;

        _context.Entry(device).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await DeviceExistsAsync(id))
                return null;
            throw;
        }

        return device;
    }

    public async Task<bool> DeleteDeviceAsync(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return false;

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> WakeOnLanAsync(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return false;

        try
        {
            WakeOnLan.WakeUp(device.MacAddress, device);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> PingAsync(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return false;

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(device.IP, 1000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> FindIpByMacAsync(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return false;

        try
        {
            var ip = await ArpLookup.FindIpFromMacAddressAsync(device.MacAddress);
            if (ip != null)
            {
                device.IP = ip;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> DeviceExistsAsync(Guid id)
    {
        return await _context.Devices.AnyAsync(e => e.Id == id);
    }
}

// You'll need to implement these helper classes:

public static class WakeOnLan
{
    
    private static readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("WakeOnLan");
    public static void WakeUp(string macAddress, Device device)
    {
        
        if (string.IsNullOrEmpty(macAddress))
            throw new ArgumentNullException(nameof(macAddress));

        try
        {
            byte[] magicPacket = CreateMagicPacket(macAddress);
            SendWakeOnLanPacket(magicPacket, device);
            _logger.LogInformation($"Wake-on-LAN packet sent to {macAddress}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending Wake-on-LAN packet to {macAddress}");
            throw;
        }
    }

    private static byte[] CreateMagicPacket(string macAddress)
    {
        // Remove any separators from the MAC address
        macAddress = new string(macAddress.Where(c => char.IsLetterOrDigit(c)).ToArray());

        if (macAddress.Length != 12)
            throw new ArgumentException("Incorrect MAC address format", nameof(macAddress));

        // Convert the MAC address to bytes
        byte[] macAddressBytes = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            macAddressBytes[i] = Convert.ToByte(macAddress.Substring(i * 2, 2), 16);
        }

        // Create the magic packet
        using (var ms = new System.IO.MemoryStream())
        {
            // Add 6 bytes of 0xFF
            ms.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0, 6);

            // Repeat MAC address 16 times
            for (int i = 0; i < 16; i++)
            {
                ms.Write(macAddressBytes, 0, 6);
            }

            return ms.ToArray();
        }
    }

    private static void SendWakeOnLanPacket(byte[] magicPacket, Device device)
    {
        // WoL is typically sent as a UDP packet to port 9
        int WOL_PORT = device.MagicPacketPort;

        using (var client = new UdpClient())
        {
            client.EnableBroadcast = true;

            // Send the packet to the broadcast address
            var endpoint = new IPEndPoint(IPAddress.Broadcast, WOL_PORT);
            client.Send(magicPacket, magicPacket.Length, endpoint);
        }
    }

    public static async Task WakeUpAsync(string macAddress)
    {
        if (string.IsNullOrEmpty(macAddress))
            throw new ArgumentNullException(nameof(macAddress));

        byte[] magicPacket = CreateMagicPacket(macAddress);

        await SendWakeOnLanPacketAsync(magicPacket);
    }

    private static async Task SendWakeOnLanPacketAsync(byte[] magicPacket)
    {
        const int WOL_PORT = 9;

        using (var client = new UdpClient())
        {
            client.EnableBroadcast = true;

            var endpoint = new IPEndPoint(IPAddress.Broadcast, WOL_PORT);
            await client.SendAsync(magicPacket, magicPacket.Length, endpoint);
        }
    }
}

public static class ArpLookup
{
    public static async Task<string> FindIpFromMacAddressAsync(string macAddress)
    {
        // Normalize the MAC address format
        macAddress = macAddress.Replace(":", "-").ToUpper();

        // Run the ARP command
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "arp",
                Arguments = "-a",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        // Parse the output
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains(macAddress))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && IPAddress.TryParse(parts[0], out _))
                {
                    return parts[0];
                }
            }
        }

        return null;
    }
}

