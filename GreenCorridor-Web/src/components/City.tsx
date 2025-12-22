import { useMemo, useEffect, useState } from 'react'
import * as THREE from 'three'
import Hospital from './Hospital'
import { useGameStore } from '../stores/gameStore'
import { realtimeAPI } from '../services/RealtimeAPI'

export default function City() {
  const { currentMission } = useGameStore()
  const [weatherCondition, setWeatherCondition] = useState<'Clear' | 'Rain' | 'Fog' | 'Storm' | 'Clouds'>('Clear')

  useEffect(() => {
    const updateWeather = async () => {
      if (currentMission) {
        const lat = (currentMission.hospitalPosition[0] / 111000) + 13.0827
        const lon = (currentMission.hospitalPosition[2] / 111000) + 80.2707
        const weather = await realtimeAPI.getWeatherData(lat, lon)
        setWeatherCondition(weather.condition)
      }
    }
    updateWeather()
    const interval = setInterval(updateWeather, 300000)
    return () => clearInterval(interval)
  }, [currentMission])

  const buildings = useMemo(() => {
    const bldgs: Array<{ position: [number, number, number], size: [number, number, number] }> = []
    
    for (let i = 0; i < 50; i++) {
      const x = (Math.random() - 0.5) * 200
      const z = (Math.random() - 0.5) * 200
      const height = Math.random() * 20 + 5
      bldgs.push({
        position: [x, height / 2, z],
        size: [Math.random() * 10 + 5, height, Math.random() * 10 + 5],
      })
    }
    
    return bldgs
  }, [])

  return (
    <>
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, 0, 0]} receiveShadow>
        <planeGeometry args={[1000, 1000]} />
        <meshStandardMaterial 
          color={weatherCondition === 'Rain' ? '#3a3a3a' : weatherCondition === 'Fog' ? '#5a5a5a' : '#4a4a4a'}
          roughness={weatherCondition === 'Rain' ? 0.3 : 0.7}
          metalness={weatherCondition === 'Rain' ? 0.1 : 0}
        />
      </mesh>

      {buildings.map((bldg, i) => (
        <mesh key={i} position={bldg.position} castShadow receiveShadow>
          <boxGeometry args={bldg.size} />
          <meshStandardMaterial
            color={`hsl(${Math.random() * 60 + 180}, 30%, ${Math.random() * 20 + 50}%)`}
          />
        </mesh>
      ))}

      <Hospital />
    </>
  )
}

