import { Canvas } from '@react-three/fiber'
import { OrbitControls, PerspectiveCamera } from '@react-three/drei'
import { Suspense, useState, useEffect } from 'react'
import GameScene from './components/GameScene'
import HUD from './components/HUD'
import MobileHUD from './components/MobileHUD'
import RealtimeGraphics from './components/RealtimeGraphics'
import HowToPlay from './components/HowToPlay'
import MainMenu from './components/MainMenu'
import GameOver from './components/GameOver'
import AIIntegration from './components/AIIntegration'
import { useGameStore } from './stores/gameStore'
import './App.css'

function App() {
  const { isPaused, togglePause, gameStatus } = useGameStore()
  const [showHelp, setShowHelp] = useState(false)

  useEffect(() => {
    const handleKeyPress = (e: KeyboardEvent) => {
      if (e.key.toLowerCase() === 'h' && gameStatus === 'playing') {
        setShowHelp(!showHelp)
      }
      if (e.key === 'Escape' && gameStatus === 'playing') {
        togglePause()
      }
    }

    window.addEventListener('keydown', handleKeyPress)
    return () => window.removeEventListener('keydown', handleKeyPress)
  }, [showHelp, gameStatus, togglePause])

  return (
    <div className="app-container">
      {gameStatus === 'menu' && <MainMenu />}
      
      {(gameStatus === 'playing' || gameStatus === 'paused') && (
        <>
          <AIIntegration />
          <Canvas
            shadows
            gl={{ antialias: true, alpha: false }}
            onCreated={({ gl }) => {
              gl.setClearColor('#87CEEB')
            }}
          >
            <Suspense fallback={null}>
              <PerspectiveCamera makeDefault position={[0, 50, 100]} fov={60} />
              <ambientLight intensity={0.5} />
              <directionalLight
                position={[50, 100, 50]}
                intensity={1}
                castShadow
                shadow-mapSize-width={2048}
                shadow-mapSize-height={2048}
              />
              <GameScene />
              <OrbitControls
                enablePan={true}
                enableZoom={true}
                enableRotate={true}
                minDistance={20}
                maxDistance={500}
              />
            </Suspense>
          </Canvas>
          {window.innerWidth < 1024 ? <MobileHUD /> : <HUD />}
          <RealtimeGraphics />
          
          {showHelp && (
            <HowToPlay onClose={() => setShowHelp(false)} />
          )}

          {isPaused && !showHelp && (
            <div className="pause-overlay">
              <div className="pause-menu">
                <h2>⏸️ PAUSED</h2>
                <p>Press ESC to resume</p>
                <button onClick={() => { setShowHelp(true); togglePause(); }}>
                  How to Play
                </button>
                <button onClick={() => { togglePause(); }}>
                  Resume Game
                </button>
              </div>
            </div>
          )}
        </>
      )}

      {gameStatus === 'won' && (
        <GameOver type="won" />
      )}

      {gameStatus === 'lost' && (
        <GameOver type="lost" reason="Mission failed" />
      )}
    </div>
  )
}

export default App

