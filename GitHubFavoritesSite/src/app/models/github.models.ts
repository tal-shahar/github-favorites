export interface RepositorySearchResult {
  repoId: string;
  name: string;
  owner: string;
  description: string;
  stars: number;
  updatedAtUtc: string;
}

export interface FavoriteRequest {
  repoId: string;
  name: string;
  owner: string;
  description: string;
  stars: number;
  updatedAtUtc: string;
}

export interface FavoriteResponse {
  id: string;
  repoId: string;
  name: string;
  owner: string;
  description: string;
  stars: number;
  updatedAtUtc: string;
  createdAtUtc: string;
  analysis?: AnalysisDto;
}

export interface AnalysisDto {
  license: string;
  topics: string[];
  languages: Record<string, number>;
  primaryLanguage: string;
  readmeLength: number;
  openIssues: number;
  forks: number;
  starsSnapshot: number;
  activityDays: number;
  defaultBranch: string;
  healthScore: number;
}

