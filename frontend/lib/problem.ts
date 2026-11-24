import type { ApiProblem } from "./types";

export async function readProblem(response: Response): Promise<ApiProblem> {
  try {
    return (await response.json()) as ApiProblem;
  } catch {
    return {};
  }
}

export function problemText(problem: ApiProblem): string {
  const messages = Object.values(problem.errors ?? {}).flat();
  if (messages.length > 0) {
    return messages.join(" ");
  }

  return problem.detail ?? "The request could not be completed.";
}
