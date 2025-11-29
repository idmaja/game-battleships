![GAME SETUP](https://res.cloudinary.com/dmsvn9nzs/image/upload/v1764419694/Screenshot_2025-11-29_193331_sv2lwx.png)
![BATTLE](https://res.cloudinary.com/dmsvn9nzs/image/upload/v1764229598/Screenshot_2025-11-27_143920_hv7w1m.png)

# Battleships Game Project

This repository contains an implementation of the classic **Battleships** game, built using .NET (C#) and React. It features multiple versions of the game, ranging from a CLI-based Console version to a fully interactive Web App (Player vs Computer).

## 📂 Project Structure

The repository is organized into four main folders, each serving a specific purpose:

### 1. `BattleshipsWebAppsVsComputer` (✅ Main Backend)

This is the **completed (final version)** backend API for the Web App game mode.

- **Mode:** Player vs Computer (PvE).
- **Tech Stack:** ASP.NET Core Web API (.NET 8), SignalR (for real-time notifications), Serilog (for logging).
- **Functionality:** Handles game logic, the computer's attack algorithm, and data communication with the frontend.

### 2. `frontend/fe-battleships-react` (✅ Main Frontend)

The user interface (UI) built with React.

- **Functionality:** Connects to and consumes the API from the `BattleshipsWebAppsVsComputer` folder.
- **Key Features:**
    - **Drag & Drop** ship placement (powered by `@dnd-kit`).
    - Interactive game board visualization.
    - Real-time notifications using the **SignalR Client**.
    - Responsive design styled with **Tailwind CSS**.

### 3. `BattleshipsConsole` (🖥️ Console Version)

A pure logic implementation of Battleships that runs in a terminal/command prompt.

- **Status:** Functional.
- **Functionality:** Ideal for testing the core game logic without a web interface.

### 4. `BattleshipsWebApps` (🚧 Planned)

This project is **planned** for a 2-Player (PvP) online mode.

- **Status:** *Work in Progress* / Not fully executed.
- **Goal:** Intended to be the backend for Human vs Human gameplay.

---

## 🛠️ Technologies Used

**Backend (.NET):**

- C# (.NET 8)
- ASP.NET Core Web API
- SignalR (Real-time Communication)
- Serilog (Logging)
- Swagger (API Documentation)

**Frontend (React):**

- React.js
- Tailwind CSS (Styling)
- Axios (HTTP Client)
- @dnd-kit (Drag and Drop utilities)
- @microsoft/signalr (WebSocket Client)
- Phosphor Icons

---

## 🚀 How to Run the Project (Web Version)

To play the Web App version (Player vs Computer), you need to run both the Backend and the Frontend simultaneously.

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or later)

### Step 1: Run the Backend

1. Open your terminal and navigate to the backend folder
    
```
cd BattleshipsWebAppsVsComputer
```
    
2. Run the application
    
```
dotnet run
```

> *The backend typically starts at `http://localhost:5069` (configured in `launchSettings.json`).*
> 

### Step 2: Configure the Frontend

Before running the frontend, ensure the API URL matches your local backend address.

1. Open `frontend/fe-battleships-react/src/services/api.js`.
2. Check the `API_BASE_URL` variable. If running locally, ensure it points to localhost:JavaScript
    
```
const API_BASE_URL = 'http://localhost:5069/api/v1/battleships';
```
    
3. Do the same for the SignalR connection in `MainLayout.jsx`: `.withUrl('http://localhost:5069/gameHub')`
    

### Step 3: Run the Frontend

1. Open a new terminal window and navigate to the frontend folder
    
```
cd frontend/fe-battleships-react
```
    
2. Install dependencies (if you haven't already)
    
```
npm install
```
    
3. Start the React application
    
```
npm start
```
    
4. Open your browser and go to `http://localhost:3000`.

---

## 🎮 How to Run the Console Version

If you want to test the game logic via the CLI:

1. Navigate to the console folder
    
```
cd BattleshipsConsole
```
    
2. Run the application
    
```
dotnet run
```
    

---

## 📝 Additional Notes

- **BattleshipsWebAppsVsComputer** includes a Swagger UI, accessible to explore the available API endpoints.
- The computer algorithm in the PvE version performs random shots but includes logic to search surrounding areas upon achieving a successful *hit*.

---

## 📄 License

This project is licensed under the **Creative Commons Attribution-NonCommercial 4.0 International Public License**. See the [LICENSE](https://github.com/idmaja/game-battleships?tab=License-1-ov-file#readme) file for details.

---

**Author:** [idmaja](https://github.com/idmaja)
