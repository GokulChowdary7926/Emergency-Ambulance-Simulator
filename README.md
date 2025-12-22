# 🚑 Green Corridor: The Emergency Ambulance Simulator You Can Play Right in Your Browser

Ever wondered what it feels like to drive an emergency ambulance through city traffic, lights flashing and siren blaring, with someone's life in your hands? **Green Corridor** lets you experience exactly that - a thrilling, realistic ambulance simulator that runs completely in your web browser.

## 🎮 What Exactly Is This Game?

Imagine this: You're an emergency medical driver. Your phone buzzes with a dispatch alert - "Cardiac emergency, 5 minutes away." You jump into your ambulance, hit the siren, and watch as traffic signals magically turn green ahead of you (that's the "green corridor" system real emergency vehicles use!). All while monitoring your patient's vital signs on the dashboard, deciding whether to administer oxygen or prepare for CPR, and navigating the fastest route to the hospital.

**Green Corridor** is more than just a driving game - it's a **pressure-packed medical simulation** that makes you think like a real EMT. The clock is ticking, the patient's condition is deteriorating, and every traffic light you catch red could mean the difference between life and death.

## 🔧 The Tech Magic Behind the Scenes

What makes this game special isn't just the gameplay - it's how it's built:

### **🎨 The 3D Engine (Three.js + React)**
Every building, ambulance, and traffic light you see is generated in real-time using **Three.js**, the same technology behind many professional 3D websites. The ambulance isn't a pre-made model - it's **programmatically built** from geometric shapes right in your browser. When it rains, the roads actually look wet. When fog rolls in, buildings fade into the mist. It's not just pretty - it affects gameplay too!

### **🗺️ Real Maps, Real Navigation (Leaflet + OpenStreetMap)**
That map you're following? It's **real map data** from OpenStreetMap. Your ambulance shows up as a moving GPS dot, just like in real navigation apps. The hospital locations, routes, and distances are all calculated using actual mapping algorithms.

### **🧠 The AI That Makes It Feel Alive**
This is where it gets really cool:
- **Smart Traffic**: Cars don't just randomly move. They "see" your ambulance and react - some pull over immediately, others hesitate (because let's face it, some drivers are slow to notice!)
- **Learning Difficulty**: The game watches how you play. Doing too well? It'll throw more challenges your way. Struggling? It'll ease up. It's like having a game master adjusting the experience just for you.
- **Traffic Predictor**: Using **TensorFlow.js** (Google's machine learning library that runs in browsers), the game predicts traffic patterns. Rush hour downtown? The game knows and makes it harder.

### **🎤 Talk to Your Ambulance (Web Speech API)**
On supported browsers, you can literally **talk to the game**: Say "turn left" and it turns left. "Activate siren" and the lights flash. It's optional, but when it works, it feels like magic.

## 🕹️ How to Play (It's Easier Than It Looks)

### **The Basics:**
1. **Pick a Mission** - Start with "Easy" (a simple transport) and work up to "Expert" (multiple patients, storm conditions)
2. **Drive to Emergency** - Use arrow keys or WASD. The map shows you where to go.
3. **Activate Emergency Mode** - Hit SPACE when you need to clear traffic. Watch signals turn green 250 meters ahead of you!
4. **Monitor Patient** - Keep an eye on the vitals panel. Heart rate dropping? Click the oxygen button.
5. **Race to Hospital** - Balance speed with safety. Crash and you fail.

### **Pro Tips:**
- **Emergency mode clears your path** but uses fuel faster
- **Rain makes roads slippery** - brake earlier
- **Check vitals at every red light** (if you get any!)
- **The map shows traffic density** - avoid red zones
- **Voice commands work best in Chrome** with a microphone

## 🏥 More Than Just Driving

What sets Green Corridor apart is the **medical simulation**:
- **Realistic vital signs** that change based on time and treatment
- **Multiple treatments** (oxygen, CPR, bleeding control) with different effects
- **Time-pressure** - some conditions deteriorate faster than others
- **Score based on both speed AND patient outcome**

## 🚀 Try It Now - No Download Needed

The craziest part? **This all runs in your browser.** No downloads, no installs, no powerful gaming PC required. We've optimized everything:
- The ML models are tiny (<1MB)
- 3D graphics use efficient rendering
- Even older computers should run it smoothly

**Perfect for:**
- Gamers who want something different from shooters
- Students interested in emergency services
- Anyone who enjoys simulation games
- People curious about AI/ML in games

## 🌟 Why I Built This

**This isn't just a game.** This is a proof-of-concept for something much bigger - **a real-time emergency response system that could save actual lives.** Every day in cities around the world, ambulances get stuck in traffic while patients' conditions deteriorate. Doctors receive patients too late because of navigation delays. Traffic systems don't communicate with emergency vehicles efficiently. Hospital ERs aren't prepared for incoming critical cases.

In many developing regions, the **"golden hour"** (the critical first hour after trauma) is often wasted in traffic jams. People die not from their injuries, but from the delay in reaching care.


## 🤖 AI & Machine Learning Features

### **Smart NPC Behavior**
NPCs (civilian vehicles) react intelligently to your ambulance:
- Pull over when ambulance is nearby with emergency mode active
- Slow down when ambulance approaches
- Some drivers may not notice immediately (adding realism)
- Behavior adapts based on traffic density and road conditions

### **Adaptive Difficulty System**
The game automatically adjusts difficulty based on your performance:
- **Increases difficulty** when you're performing well (more traffic, tighter time limits)
- **Decreases difficulty** when you're struggling (clearer paths, more time)
- Tracks metrics including success rate, response time, and patient health outcomes
- Ensures the game remains challenging but fair

### **ML Traffic Prediction**
The game uses TensorFlow.js to predict traffic density:
- Neural network trained on realistic traffic patterns
- Considers multiple factors: time of day, day of week, weather conditions, area type
- Updates predictions every 30 seconds during gameplay
- Runs entirely in the browser without requiring a backend

### **Voice Control System**
Optional voice command support using the Web Speech API:
- Works in Chrome, Edge, and Safari (with permissions)
- Continuous listening mode for hands-free control
- Natural language command recognition
- Can be enabled/disabled in game settings

## 🌐 Browser Compatibility

- **Chrome/Edge**: Full support including all features and voice commands
- **Firefox**: Full support, voice commands may vary by version
- **Safari**: Full support, voice commands require user permission
- **Mobile Browsers**: Responsive design with optimized touch controls

## ⚡ Performance Optimization

The game is optimized for performance:
- TensorFlow.js models are lightweight (<1MB) and load quickly
- 3D models use efficient geometries to maintain smooth frame rates
- State management is optimized with Zustand for minimal re-renders
- Map tiles are loaded on demand to reduce initial load time
- Code splitting is implemented for faster initial page loads

## 🛠️ Technology Stack

### **Core Framework**
- **React 18.2.0** - Modern, component-based UI framework
- **TypeScript 5.3.3** - Type-safe JavaScript with enhanced IDE support
- **Vite 5.0.8** - Lightning-fast build tool and development server

### **3D Graphics & Rendering**
- **Three.js 0.158.0** - Industry-standard 3D graphics library for WebGL rendering
- **React Three Fiber 8.15.0** - Declarative React renderer for Three.js
- **@react-three/drei 9.88.0** - Comprehensive helpers and abstractions for Three.js

### **State Management**
- **Zustand 4.4.7** - Lightweight, fast state management solution

### **Maps & Location Services**
- **Leaflet 1.9.4** - Open-source JavaScript library for interactive maps
- **React Leaflet 4.2.1** - React components for Leaflet integration
- **OpenStreetMap** - Free, open-source map tiles

### **Machine Learning & AI**
- **TensorFlow.js 4.22.0** - Browser-based machine learning for traffic prediction
- **Web Speech API** - Native browser API for voice recognition

### **HTTP & API Communication**
- **Axios 1.13.2** - Promise-based HTTP client for API requests

### **Development Tools**
- **@vitejs/plugin-react 4.2.1** - Vite plugin for React support

## 🎨 Models & Components

### **3D Models**
The game uses procedurally generated 3D models:

- **Ambulance**: Custom 3D model with emergency lights, and wheels
- **Hospital**: Distinctive building model with red cross marker
- **City Buildings**: 50 procedurally generated buildings with random heights
- **Traffic Signals**: 3D traffic light models with state-based colors
- **Ground Plane**: Large terrain mesh with weather-affected materials

### **UI Components**
- **HUD**: Desktop dashboard with speed, RPM, fuel gauges, patient vitals
- **MobileHUD**: Mobile-optimized interface with touch-friendly controls
- **VitalsMonitor**: Patient health monitoring panel
- **CommunicationPanel**: Radio messages and dispatch updates
- **TrafficControlPanel**: Traffic signal status indicators
- **LeafletMap**: Interactive map with ambulance and hospital markers
- **MainMenu**: Mission selection screen
- **GameOver**: Win/lose screen with statistics
- **HowToPlay**: In-game help guide

### **Game Systems**
- **GameScene**: Main 3D scene orchestrator
- **Ambulance**: Vehicle controller with physics-based movement
- **City**: Procedural city generation
- **TrafficSignals**: Traffic light management with 250m preemption radius
- **RealtimeGraphics**: Weather effects and visual enhancements
- **RealtimeAPI**: Weather, traffic, and GPS data services

### **AI & ML Systems**
- **SimpleNPCController**: Smart NPC behavior with emergency awareness
- **AdaptiveDifficulty**: Dynamic difficulty adjustment
- **TrafficPredictor**: ML-powered traffic prediction
- **VoiceController**: Voice command recognition
- **AIIntegration**: Main integration component managing all AI systems

---
**Next time you play Green Corridor, remember:** You're not just navigating virtual streets. You're testing a system that could one day clear a real path for a real ambulance carrying a real patient whose life depends on every second saved.

**Ready to save some lives?** The ambulance is waiting, the dispatch is calling, and someone out there needs you. How fast can you get there? 🚨

**[Play now and experience emergency medical response like never before!]**