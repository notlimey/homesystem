"use client";

import clsx from "clsx";
import useJobs from "../../common/hooks/jobs/use-jobs";

export default function Home() {
	const { data, isPending } = useJobs();

	return (
		<>
			<div className="flex flex-col gap-y-4 max-w-[1440px] px-5 mx-auto py-8">
				<h1 className="scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl">
					Martinlime homesystem
				</h1>
				<div className="flex flex-col gap-y-2">
					<h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
						Jobs:
					</h3>
					{isPending && <p>Loading...</p>}
					{data?.map((job) => (
						<div key={job.name} className="flex items-center gap-x-2">
							<div
								className={clsx(
									job.isRunning ? "bg-green-500" : "bg-red-500",
									"w-2 h-2 rounded-full",
								)}
							/>
							{job.name}
						</div>
					))}
				</div>
			</div>
		</>
	);
}
