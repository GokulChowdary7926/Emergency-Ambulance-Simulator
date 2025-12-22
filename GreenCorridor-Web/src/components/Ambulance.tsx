import { useRef, useEffect } from 'react'
import { useFrame } from '@react-three/fiber'
import { useGameStore } from '../stores/gameStore'
import * as THREE from 'three'

export default function Ambulance() {
  const meshRef = useRef<THREE.Group>(null)
  const { ambulancePosition, updateAmbulance, activateEmergency, isEmergencyActive, currentMission } = useGameStore()
  const keys = useRef<Set<string>>(new Set())
  const velocity = useRef(new THREE.Vector3())
  const position = useRef(new THREE.Vector3(...ambulancePosition))

  useEffect(() => {
    if (currentMission) {
      position.current.set(...currentMission.startPosition)
      if (meshRef.current) {
        meshRef.current.position.copy(position.current)
      }
    }
  }, [currentMission])

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key.toLowerCase() === 'h' || e.key === 'Escape') {
        return
      }
      keys.current.add(e.key.toLowerCase())
      if (e.key === ' ') {
        e.preventDefault()
        activateEmergency()
      }
    }
    const handleKeyUp = (e: KeyboardEvent) => {
      if (e.key.toLowerCase() === 'h' || e.key === 'Escape') {
        return
      }
      keys.current.delete(e.key.toLowerCase())
    }

    window.addEventListener('keydown', handleKeyDown)
    window.addEventListener('keyup', handleKeyUp)

    return () => {
      window.removeEventListener('keydown', handleKeyDown)
      window.removeEventListener('keyup', handleKeyUp)
    }
  }, [activateEmergency])

  useFrame((_state, delta) => {
    if (!meshRef.current) return

    const maxSpeed = isEmergencyActive ? 120 : 80
    const acceleration = 15
    const turnSpeed = 2
    const brakeForce = 30

    let throttle = 0
    let brake = 0
    let turn = 0

    if (keys.current.has('w') || keys.current.has('arrowup')) throttle = 1
    if (keys.current.has('s') || keys.current.has('arrowdown')) brake = 1
    if (keys.current.has('a') || keys.current.has('arrowleft')) turn = -1
    if (keys.current.has('d') || keys.current.has('arrowright')) turn = 1

    if (throttle > 0) {
      velocity.current.z = Math.min(velocity.current.z + acceleration * delta, maxSpeed)
    } else if (brake > 0) {
      velocity.current.z = Math.max(velocity.current.z - brakeForce * delta, 0)
    } else {
      velocity.current.z = Math.max(velocity.current.z - 5 * delta, 0)
    }

    if (Math.abs(velocity.current.z) > 0.1) {
      meshRef.current.rotation.y += turn * turnSpeed * delta * (velocity.current.z / maxSpeed)
    }

    const direction = new THREE.Vector3(0, 0, 1)
    direction.applyQuaternion(meshRef.current.quaternion)
    position.current.add(direction.multiplyScalar(velocity.current.z * delta))

    meshRef.current.position.copy(position.current)

    const speedKMH = velocity.current.z * 3.6
    const heading = meshRef.current.rotation.y
    updateAmbulance(
      [position.current.x, position.current.y, position.current.z],
      speedKMH,
      heading
    )
  })

  return (
    <group ref={meshRef} position={ambulancePosition}>
      {/* Ambulance body */}
      <mesh castShadow receiveShadow>
        <boxGeometry args={[3, 2, 6]} />
        <meshStandardMaterial color={isEmergencyActive ? '#ff0000' : '#ffffff'} />
      </mesh>
      
      {/* Emergency lights */}
      {isEmergencyActive && (
        <>
          <mesh position={[1.5, 1.5, 2]}>
            <boxGeometry args={[0.3, 0.3, 0.3]} />
            <meshStandardMaterial color="#ff0000" emissive="#ff0000" emissiveIntensity={2} />
          </mesh>
          <mesh position={[-1.5, 1.5, 2]}>
            <boxGeometry args={[0.3, 0.3, 0.3]} />
            <meshStandardMaterial color="#0000ff" emissive="#0000ff" emissiveIntensity={2} />
          </mesh>
        </>
      )}
      
      {/* Wheels */}
      {[-2, 2].map((x) => (
        <mesh key={x} position={[x, -1, -2]} castShadow>
          <cylinderGeometry args={[0.5, 0.5, 0.3]} />
          <meshStandardMaterial color="#222" />
        </mesh>
      ))}
    </group>
  )
}

