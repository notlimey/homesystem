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
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(HomeDbContext context, ILogger<DeviceService> logger)
    {
        _context = context;
        _logger = logger;
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
        {
            _logger.LogWarning($"Device with id {id} not found");
            return false;
        }

        try
        {
            _logger.LogInformation($"Attempting to wake device {device.Name} with MAC {device.MacAddress}");
        
            await WakeOnLan.WakeUpAsync(device.MacAddress, device);
        
            // Wait for the device to wake up
            for (int i = 0; i < 5; i++)
            {
                _logger.LogInformation($"Checking if device {device.Name} is online (attempt {i + 1}/5)");
                await Task.Delay(5000);
            
                if (await PingAsync(id))
                {
                    _logger.LogInformation($"Device {device.Name} is now online");
                    return true;
                }
                _logger.LogInformation($"Device {device.Name} not responding to ping yet");
            }
    
            _logger.LogWarning($"Device {device.Name} did not respond after wake attempt");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending Wake-on-LAN packet to {device.Name}");
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
    
    public static async Task WakeUpAsync(string macAddress, Device device)
    {
        if (string.IsNullOrEmpty(macAddress))
            throw new ArgumentNullException(nameof(macAddress));

        try
        {
            byte[] magicPacket = CreateMagicPacket(macAddress);
            
            // Use the global broadcast address 255.255.255.255
            var broadcastAddress = IPAddress.Broadcast;
            
            _logger.LogInformation($"Sending magic packet to 255.255.255.255:9 with payload {macAddress}");
            
            await SendWakeOnLanPacketAsync(magicPacket, broadcastAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending Wake-on-LAN packet to {macAddress}");
            throw;
        }
    }

    private static async Task SendWakeOnLanPacketAsync(byte[] magicPacket, IPAddress targetAddress)
    {
        using (var client = new UdpClient())
        {
            client.EnableBroadcast = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            
            var endpoint = new IPEndPoint(targetAddress, 9); // Using port 9
            
            // Send the packet multiple times for reliability
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await client.SendAsync(magicPacket, magicPacket.Length, endpoint);
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to send WoL packet to {targetAddress}:9 - {ex.Message}");
                }
            }
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

        // Create the magic packet (102 bytes)
        byte[] magicPacket = new byte[102];
        
        // First 6 bytes should be 0xFF
        for (int i = 0; i < 6; i++)
            magicPacket[i] = 0xFF;
        
        // Repeat MAC address 16 times (16 * 6 = 96 bytes)
        for (int i = 1; i <= 16; i++)
            Array.Copy(macAddressBytes, 0, magicPacket, i * 6, 6);

        return magicPacket;
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

