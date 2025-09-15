# SonarCloud CI setup for `ppanchen/NetSdrClient`

This folder contains a minimal **GitHub Actions + SonarCloud** setup tailored to your repo.

## What to do
1. Add **`SONAR_TOKEN`** in GitHub → *Settings → Secrets and variables → Actions → New repository secret*.
2. Drop these files into the root of `ppanchen/NetSdrClient`:
   - `.github/workflows/ci.yml`
   - `sonar-project.properties`
3. (Optional) Enable **Branch protection** for `main` and mark **required checks**: the CI job and *SonarCloud Code Analysis / Quality Gate*.
4. Open a PR – the workflow will build, run tests with coverage, and publish analysis to SonarCloud.

## Keys used
- `sonar.organization`: **ppanchen** (adjust if your SonarCloud org key is different)
- `sonar.projectKey`: **ppanchen_NetSdrClient** (recommended; adjust if SonarCloud shows a different key)

If SonarCloud displays other values on your project's page, update them in `ci.yml` and/or `sonar-project.properties`.
