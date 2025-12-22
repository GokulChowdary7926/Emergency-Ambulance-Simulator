import { useGameStore } from '../stores/gameStore'

export default function Hospital() {
  const { currentMission } = useGameStore()
  const position = currentMission?.hospitalPosition || [100, 0, 100]

  return (
    <group position={position}>
      <mesh castShadow receiveShadow>
        <boxGeometry args={[20, 20, 20]} />
        <meshStandardMaterial color="#ffffff" />
      </mesh>
      <mesh position={[0, 25, 0]}>
        <coneGeometry args={[5, 10, 4]} />
        <meshStandardMaterial color="#ff0000" />
      </mesh>
      {/* Red cross on front */}
      <mesh position={[0, 10, 10.1]}>
        <planeGeometry args={[12, 12]} />
        <meshStandardMaterial color="#ff0000" />
      </mesh>
    </group>
  )
}


