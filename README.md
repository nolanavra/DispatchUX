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
   - Create a **Site Marker** prefab (Image + TMP label) with `SiteMarker` for showing imported cafe / business locations.
   - Add `CafeDatabaseLoader` to the scene (or Resources) so cafe/business JSON is available at runtime.
   - Add `MapViewController` to the map panel. Assign **DispatchDataManager**, **MapArea** (RectTransform of panel), and all three marker prefabs (site, technician, job). The controller now projects every cafe/business from the dataset plus generated technicians and jobs using their latitude/longitude so map positions match real-world spacing.

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

## Phase 2 Wiring (Workload, Recommendations, Notes, Routes)
1. **Workload Balance Meter**
   - Add a small bar (Image + TMP label) inside the Technician Card prefab and attach `TechnicianWorkloadMeterUI` to it.
   - Assign the meter’s **FillImage** and **WorkloadLabel** fields, then assign the meter to `TechnicianCardUI.WorkloadMeter` so it binds to `DispatchDataManager` and refreshes from workload events.

2. **Communication Panel**
   - Create a reusable panel with header text, scrollable notes content (using a simple text prefab), input field, and Add/Close buttons.
   - Attach `CommunicationPanelUI` to the panel root and wire **PanelRoot**, **HeaderText**, **NotesContentRoot**, **NoteEntryPrefab**, **InputField**, **AddNoteButton**, and **CloseButton**.
   - On the Technician Card prefab, add a “Notes” button hooked to `TechnicianCardUI.NotesButton` and assign the shared `CommunicationPanelUI` reference inside `TechnicianListUIController`.
   - On the Job Card prefab, add a “Notes” button hooked to `JobCardUI.NotesButton` and assign the same panel reference in `JobListUIController`.

3. **Job Recommendations & Highlighting**
   - Place a `JobRecommendationService` in the scene and assign **DispatchDataManager**.
   - Add `TechnicianHighlightController` in the scene, assigning both the `TechnicianListUIController` (for card highlights) and `MapViewController` (for marker highlights).
   - Add a “Recommend” button to the Job Card prefab, wire it to `JobCardUI.RecommendButton`, and in `JobListUIController` set **RecommendationService** and **HighlightController** so clicks highlight the suggested technicians.

4. **Auto-Generated Daily Routes**
   - Add `RoutePlannerService` to the scene and assign **DispatchDataManager**.
   - Create a UI button labeled “Generate Daily Routes”; attach `RoutePlanningController` to the same GameObject, assign **routePlannerService** (and optionally a `TechnicianHighlightController` to clear highlights), then wire the button’s OnClick to `RoutePlanningController.GenerateRoutes`.
   - Ensure `TimelineUIController` is already referencing **DispatchDataManager**, row/job prefabs, and content root; it will reorder jobs when `DispatchDataManager.OnRoutesGenerated` fires after route planning.

## Phase 3 Wiring (Detail Panels)
1. **Create Detail Panels**
   - Under the main Canvas, add **TechnicianDetailPanel** and **JobDetailPanel** GameObjects. Attach `TechnicianDetailPanelUI` and `JobDetailPanelUI` respectively and assign their **panelRoot** fields to the panel GameObjects.
   - For each panel, wire TMP text fields, scroll content roots, and prefabs: `SkillEntryUI`, `AssignedJobEntryUI`, `RequiredSkillToggleUI`, and `NoteEntryUI`.

2. **Communication Panel Hooks**
   - Drag the shared `CommunicationPanelUI` into the **communicationPanel** field on both detail panels.
   - Add buttons to the panels if desired and hook them to `TechnicianDetailPanelUI.OnOpenCommunicationForTechnician` and `JobDetailPanelUI.OnOpenCommunicationForJob/OnOpenCommunicationForTechnician`.

3. **List Controllers and Cards**
   - In `TechnicianListUIController`, assign the **TechnicianDetailPanel** reference so spawned `TechnicianCardUI` instances can open it on click.
   - In `JobListUIController`, assign **JobDetailPanel** and ensure the Job Card prefab has its `JobCardUI.JobDetailPanel` field exposed (the controller fills it at runtime).

4. **Map Markers**
   - On `MapViewController`, set **TechnicianDetailPanel** and **JobDetailPanel** references. Marker prefabs should include `TechnicianMarker` and `JobMarker` components; leave their panel references empty so the controller injects them when instantiating.
   - Confirm markers have raycast targets so `IPointerClickHandler` events fire and open the panels.

5. **Timeline Blocks**
   - Assign **JobDetailPanel** on `TimelineUIController` and ensure the Job Block prefab’s click handler (Button or EventTrigger) calls `TimelineJobBlockUI.OnClick` to show job details.

6. **Notes and Required Skills UI**
   - Place `RequiredSkillToggleUI` inside a vertical/horizontal layout group under the Job detail’s **requiredSkillsContainer**; toggling updates the underlying `RequiredSkillToggle` data.
   - Place `NoteEntryUI` prefabs under each panel’s notes container to display ordered job/technician notes. Use the Communication panel to add new notes; the detail panels simply display them.

## Real-World Map Seeding
- When a `CafeDatabaseLoader` is present (or Resources/cafes.json is available), `DispatchDataManager` seeds technicians and jobs from the imported latitude/longitude data and projects every site onto the map with consistent bounds.
- `MapViewController` now spawns **SiteMarker** instances for every cafe/business entry alongside technician and job markers so the canvas reflects real-world spacing instead of placeholder 0–100 coordinates.
