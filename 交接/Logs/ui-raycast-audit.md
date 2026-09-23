# Raycast reachability audit (read-only)

Buttons in scene = 169 ; unreachable candidates = 4

A button is reported when no Graphic in its scene-visible subtree has m_RaycastTarget=1,
or when an ancestor CanvasGroup has m_BlocksRaycasts=0.

NOTE: prefab-instance children are not present in the scene YAML, so a button whose clickable
      graphic lives inside a prefab instance may be reported as a false positive.

- `` -- subtree has NO graphic with raycastTarget=true
- `` -- subtree has NO graphic with raycastTarget=true
- `` -- subtree has NO graphic with raycastTarget=true
- `` -- subtree has NO graphic with raycastTarget=true

