import { useQuery } from "@tanstack/react-query";
import axios from "axios";
import type { Job } from "../../types/job.types";

export default function useJobs() {
	return useQuery<Job[]>({
		queryKey: ["jobs"],
		queryFn: () =>
			axios.get("http://localhost:5018/api/Jobs").then((res) => res.data),
	});
}
