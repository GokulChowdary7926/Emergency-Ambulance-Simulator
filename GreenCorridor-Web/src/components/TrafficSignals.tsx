import { useMemo } from 'react'
import { useGameStore } from '../stores/gameStore'
import * as THREE from 'three'

export default function TrafficSignals() {
  const { ambulancePosition, isEmergencyActive, trafficSignals } = useGameStore()

  const signals = useMemo(() => {
    const sigs: Array<{ id: number, position: [number, number, number] }> = []
    
    for (let i = 0; i < 20; i++) {
      const x = Math.floor((Math.random() - 0.5) * 200 / 20) * 20
      const z = Math.floor((Math.random() - 0.5) * 200 / 20) * 20
      sigs.push({
        id: i,
        position: [x, 2, z],
      })
    }
    
    return sigs
  }, [])

  const preemptedSignals = useMemo(() => {
    if (!isEmergencyActive) return new Set<number>()
    
    const preempted = new Set<number>()
    const ambulancePos = new THREE.Vector3(...ambulancePosition)
    
    signals.forEach((signal) => {
      const signalPos = new THREE.Vector3(...signal.position)
      const distance = ambulancePos.distanceTo(signalPos)
      if (distance < 250) {
        preempted.add(signal.id)
      }
    })
    
    return preempted
  }, [ambulancePosition, isEmergencyActive, signals])

  return (
    <>
      {signals.map((signal) => {
        const isPreempted = preemptedSignals.has(signal.id)
        const color = isPreempted ? '#00ff00' : '#ff0000'
        
        return (
          <group key={signal.id} position={signal.position}>
            {/* Pole */}
            <mesh>
              <cylinderGeometry args={[0.1, 0.1, 4]} />
              <meshStandardMaterial color="#333" />
            </mesh>
            
            {/* Signal box */}
            <mesh position={[0, 2.5, 0]}>
              <boxGeometry args={[0.5, 1.5, 0.3]} />
              <meshStandardMaterial color="#222" />
            </mesh>
            
            {/* Light */}
            <mesh position={[0, 2.5, 0.2]}>
              <sphereGeometry args={[0.15, 16, 16]} />
              <meshStandardMaterial
                color={color}
                emissive={color}
                emissiveIntensity={isPreempted ? 2 : 0.5}
              />
            </mesh>
          </group>
        )
      })}
    </>
  )
}


