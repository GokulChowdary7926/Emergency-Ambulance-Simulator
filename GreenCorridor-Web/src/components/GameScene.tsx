import { useFrame } from '@react-three/fiber'
import { useRef, useEffect, useState } from 'react'
import { useGameStore } from '../stores/gameStore'
import { realtimeAPI } from '../services/RealtimeAPI'
import Ambulance from './Ambulance'
import City from './City'
import TrafficSignals from './TrafficSignals'
import * as THREE from 'three'

export default function GameScene() {
  const { updateGameTime, isPaused, ambulancePosition, currentMission } = useGameStore()
  const hospitalPosition = useRef(new THREE.Vector3(100, 0, 100))
  const [ambientIntensity, setAmbientIntensity] = useState(0.5)
  const [fogDensity, setFogDensity] = useState(0)

  useEffect(() => {
    if (currentMission) {
      hospitalPosition.current.set(...currentMission.hospitalPosition)
      
      const updateWeatherEffects = async () => {
        const lat = (currentMission.hospitalPosition[0] / 111000) + 13.0827
        const lon = (currentMission.hospitalPosition[2] / 111000) + 80.2707
        const weather = await realtimeAPI.getWeatherData(lat, lon)
        
        if (weather.condition === 'Fog' || weather.condition === 'Storm') {
          setAmbientIntensity(0.3)
          setFogDensity(0.05)
        } else if (weather.condition === 'Rain' || weather.condition === 'Clouds') {
          setAmbientIntensity(0.4)
          setFogDensity(0.02)
        } else {
          setAmbientIntensity(0.5)
          setFogDensity(0)
        }
      }
      
      updateWeatherEffects()
      const interval = setInterval(updateWeatherEffects, 300000)
      return () => clearInterval(interval)
    }
  }, [currentMission])

  useFrame((state, delta) => {
    if (!isPaused) {
      updateGameTime(delta)
      
      const ambulancePos = new THREE.Vector3(...ambulancePosition)
      const distance = ambulancePos.distanceTo(hospitalPosition.current)
      const speed = useGameStore.getState().ambulanceSpeed / 3.6
      const eta = speed > 1 ? distance / speed : 0
      
      useGameStore.setState({ timeToHospital: eta })
    }
  })

  return (
    <>
      <fog attach="fog" args={['#87CEEB', 50, 200]} density={fogDensity} />
      <ambientLight intensity={ambientIntensity} />
      <City />
      <Ambulance />
      <TrafficSignals />
      <gridHelper args={[1000, 100, '#888', '#ccc']} />
    </>
  )
}

