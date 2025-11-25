# DispatchQuest (Phase 1 MVP)

A Unity 2D desktop-focused dispatch tool prototype with a strategy-UI aesthetic. Built with Unity 2022+ and uGUI (Canvas/RectTransform) using TextMeshPro for all text.

## Project Layout
- `Assets/Scripts/Data` — data models for technicians and jobs.
- `Assets/Scripts/Managers` — data manager that seeds dummy technicians/jobs and raises assignment events.
- `Assets/Scripts/UI` — UI controllers for job list, technician list, timeline, and draggable cards.
- `Assets/Scripts/Map` — map view controller and marker components for technicians and jobs.

## Prerequisites
- Unity **2022 LTS** or later.
- TextMeshPro package installed (Unity will prompt to import TMP essentials on first use).

## Scene Wiring (DispatchScene)
1. **Create Scene Basics**
   - Add an **EventSystem** and a **Canvas** (Screen Space - Overlay). Set reference resolution around 1920x1080 using a Canvas Scaler.
   - Under the canvas, create panels: left **Technician List** (ScrollRect), center **Map Panel** (Image as background), right **Job List** (ScrollRect), and a bottom or top **Timeline Panel**.

2. **Add DispatchDataManager**
   - Create an empty GameObject named `DispatchManager` and add `DispatchDataManager`.
   - Configure map bounds if desired (defaults to 0–100 in X/Y).

3. **Job List Panel**
   - Create a **Job Card** prefab: Root with `CanvasGroup`, background Image, and TMP texts for title, client, priority, duration, and skills. Attach `JobCardUI`.
   - In the Job List ScrollRect, set its **Content** to use a Vertical Layout Group. Add `JobListUIController` to the panel, assign **DispatchDataManager**, **JobCardPrefab**, **ContentRoot**, and **RootCanvas** (the main canvas).

4. **Technician List Panel**
   - Create a **Technician Card** prefab with background Image and TMP texts for name, skills, and status. Attach `TechnicianCardUI`.
   - In the Technician ScrollRect, set Content with Vertical Layout Group. Add `TechnicianListUIController`, assigning **DispatchDataManager**, **TechnicianCardPrefab**, and **ContentRoot**.

5. **Map Panel**
   - Create **Technician Marker** prefab (Image + TMP label) with `TechnicianMarker` component (implements drop target).
   - Create **Job Marker** prefab (Image + TMP label) with `JobMarker` component.
   - Add `MapViewController` to the map panel. Assign **DispatchDataManager**, **MapArea** (RectTransform of panel), and both marker prefabs.

6. **Timeline Panel**
   - Create **Technician Row** prefab containing a `TechLabel` TMP text and a `Jobs` container with Horizontal Layout Group.
   - Create **Job Block** prefab with background Image and TMP texts for title and duration; attach `TimelineJobBlockUI`.
   - Add `TimelineUIController` to the timeline container and assign **DispatchDataManager**, **TechnicianRowPrefab**, **JobBlockPrefab**, and **ContentRoot**.

7. **Drag-and-Drop Wiring**
   - Ensure job cards have `CanvasGroup` (for raycast toggling) and `JobCardUI` is provided the **RootCanvas** via `JobListUIController`.
   - Technician cards and technician markers already implement `IDropHandler`; no extra setup beyond assigning DataManager during instantiation.

8. **Play Mode**
   - Enter Play. Dummy technicians (4–6) and jobs (8–12) spawn automatically. Drag a job card onto a technician card or marker to assign. The job disappears from the unassigned list, updates marker colors, and shows on that technician’s timeline row.

## Notes
- All text components should be **TextMeshProUGUI**.
- No persistence is implemented; data resets each Play session.
