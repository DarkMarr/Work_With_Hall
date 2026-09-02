# UI Reference Manifest

This folder defines the Phase 1 UI Reference foundation for HALL900.

## Reference images

Put screen reference images under `Assets/Art/UI_Reference/` or its subfolders. New references can be added at any time; the auditor discovers image files when `autoDiscoverReferenceImages` is enabled.

## Manifest

`ui_manifest.json` is the stable mapping layer between a reference screen and a Unity scene. Keep entries append-only where possible.

Example:

```json
{
  "version": 1,
  "referenceRoot": "Assets/Art/UI_Reference",
  "autoDiscoverReferenceImages": true,
  "screens": [
    {
      "id": "Authentication",
      "scenePath": "Assets/Scenes/Authentication.unity",
      "referenceImage": "Assets/Art/UI_Reference/Authentication.png"
    }
  ]
}
```

Phase 1 only checks structure and declared UI elements. It does not judge artistic quality and does not replace final graphic resources.
