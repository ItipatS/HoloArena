
#  3D Arena Fighting Game – Unity Project

This project is a **3D arena-style fighting game**, built entirely in Unity with a focus on scalable architecture, clean modular code, and responsive UI systems. Inspired by classic fighting games like *Super Smash Bros.* and *Brawlhalla*, this game showcases both **gameplay mechanics** and **user interface design** — ideal for demonstrating technical depth and transferable skills to any game dev role, including Cocos Creator.

> **🛠 Built with:** Unity (C#), Component-Based Architecture, Modular Systems, Inspector-Friendly Design

---

##  Project Overview

The game features:
- A **state-driven character system** that supports modular features like movement, gravity, ground detection, and stats.
- A **UI system** for menus, pause screens, and fight outcome display, emphasizing animation, usability, and modularity.
- Smooth **gameplay control logic** based on custom modules rather than Unity’s built-in controller, designed for expandability.

---

## Project Structure & Highlights

###  Scripts (Core Game Logic)
Located in the `Scripts/` folder, the project follows a **modular, component-based approach** where each mechanic (movement, gravity, stats, attack) is implemented as its own self-contained module. This makes it easy to:
- **Extend functionality** (e.g., add new mechanics without editing existing scripts).
- **Maintain and debug** (e.g., each module has a clear responsibility).
- **Reuse across characters or games** with minimal coupling.

Notable principles:
- **Data-driven design:** Stats and values are driven from character data, promoting reusability and easier balancing.
- **Loose coupling:** Each module communicates via interfaces and is managed through a central controller.
- **Scalable logic:** New modules can be plugged into the core loop without modifying the base character structure.

- [View Scripts Folder](./Assets/_Project/Scripts)
>  Example: `GroundCheckModule`, `GravityComponent`, and `CharacterStat` are fully reusable and can be assigned per-character via the Inspector.

---

###  UI System


Located in the `UIElements/` folder, the project includes responsive UI components for:
- Main menu navigation
- Pause and resume logic
- KO screen and transitions
- Character selection screen

Highlights:
- **UI scripts are fully decoupled from logic**, making them easy to replace or animate.
- **Animator-driven buttons** enhance user feedback using `ButtonAnimatorUtility`.
- UI managers are **scene-aware** and follow naming conventions, making them easy to identify and swap.
  
- [View UI Folder](../Assets/_Project/UIElements)
>  Designed to be intuitive, readable, and extendable, especially for team handoff or designer collaboration.
---
### Visual Effects & Shader Work

- This project also includes custom shader effects to enhance player feedback and UI-less design:

- Energy levels and combat status are visualized through in-world shader effects (e.g., glow, saturation, or outline changes).

- Shader-driven feedback replaces traditional UI bars, creating a cleaner and more immersive game experience.

- Effects are tied to gameplay states (e.g., energy low = pulsing glow), and support stacking indicators for more complex mechanics.
  
- [View VFX Folder](./Assets/_Project/VFX)
> Built with Unity's Built-in Render Pipeline and custom shader scripts (without relying on URP/HDRP), designed to run efficiently on low-spec devices.

---
##  Reusability & Maintainability

This project is designed to be:
- **Scalable:** Add new mechanics or characters without rewriting existing systems.
- **Maintainable:** Follows SRP (Single Responsibility Principle) to isolate logic.
- **Readable:** Clean naming, folder structure, and comments promote quick onboarding for other developers.

---
## 📢 Disclaimer

This project uses free community assets solely for educational and demonstration purposes.
All models, music, and effects included are either created by me or under a license that allows redistribution.


## 📌 Author Note

This is a solo project built with ~1 year of Unity experience, emphasizing hands-on learning, scalable systems, and code organization. It's a work-in-progress but already demonstrates strong foundations in gameplay logic, UI flow, and clean software design.

