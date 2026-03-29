# LaundryLog Workspace

This workspace holds checked-in, human-viewable LaundryLog artifacts that should be reviewable directly from the repo and traceable through git history.

Use this area for:

- current self-contained HTML artifacts that represent the latest meaningful renderer state
- local design/reference HTML that should remain visible in repo history
- companion files that the checked-in HTML artifacts depend on, such as update manifests

Current structure:

- `html/aem-paths/`
  Adam/Event Modeling (`AEM`) HTML path artifacts
- `html/screen-paths/`
  ordered screen-path HTML artifacts
- `html/screens/`
  current screen-surface HTML artifacts
- `html/reference/`
  tracked design/reference HTML inputs that still matter

Working rule:

- `tmp/` remains scratch and can be noisier or more experimental
- this workspace holds the checked-in current artifacts worth preserving in git history
- when renderer or artifact behavior changes materially, refresh the relevant workspace artifact in the same commit as the code/docs change
- do not force separate artifact-only commits just to keep the workspace current
