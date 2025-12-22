import { useEffect, useRef } from 'react'
import { useGameStore } from '../stores/gameStore'
import { AdaptiveDifficulty } from '../ai/DifficultyManager'
import { TrafficPredictor } from '../ai/TrafficPredictor'
import { VoiceController } from '../ai/VoiceController'

let difficultyManager: AdaptiveDifficulty | null = null
let trafficPredictor: TrafficPredictor | null = null
let voiceController: VoiceController | null = null

export default function AIIntegration() {
  const {
    gameStatus,
    currentMission,
    ambulancePosition,
    isEmergencyActive,
    timeRemaining,
    patient,
    stats,
    updateAIDifficulty,
    updateAIPerformance,
    activateEmergency,
    winGame,
    loseGame
  } = useGameStore()

  const initialized = useRef(false)

  useEffect(() => {
    if (initialized.current) return

    difficultyManager = new AdaptiveDifficulty()
    trafficPredictor = new TrafficPredictor()
    voiceController = new VoiceController()

    voiceController.initialize()
    voiceController.onCommand((command) => {
      switch (command) {
        case 'TOGGLE_SIREN':
        case 'EMERGENCY_MODE':
          activateEmergency()
          break
        case 'ACCELERATE':
        case 'BRAKE':
        case 'TURN_LEFT':
        case 'TURN_RIGHT':
          break
      }
    })

    trafficPredictor.initialize().catch(console.error)

    initialized.current = true

    return () => {
      trafficPredictor?.dispose()
      voiceController?.stop()
    }
  }, [activateEmergency])

  useEffect(() => {
    if (gameStatus !== 'playing' || !currentMission) return

    const updateTrafficPrediction = async () => {
      if (!trafficPredictor) return

      const now = new Date()
      const timeOfDay = now.getHours() + now.getMinutes() / 60
      const dayOfWeek = now.getDay()
      
      const weatherMap: Record<string, number> = {
        'Clear': 0.2,
        'Rain': 0.6,
        'Fog': 0.8,
        'Storm': 0.9,
        'Clouds': 0.4
      }

      const predictedTraffic = await trafficPredictor.predictTraffic({
        timeOfDay,
        dayOfWeek,
        weather: weatherMap[currentMission.weather] || 0.5,
        areaType: currentMission.trafficDensity / 10,
        specialEvent: 0
      })

      useGameStore.setState({
        realtimeTraffic: {
          density: predictedTraffic * 100,
          flow: (1 - predictedTraffic) * 100
        }
      })
    }

    updateTrafficPrediction()
    const interval = setInterval(updateTrafficPrediction, 30000)
    return () => clearInterval(interval)
  }, [gameStatus, currentMission])

  useEffect(() => {
    if (gameStatus !== 'playing' || !difficultyManager) return

    const updatePerformance = () => {
      if (!difficultyManager) return

      const overallHealth = Math.round(
        patient.consciousness * 0.3 +
        (patient.oxygenSaturation / 100) * 30 +
        (Math.max(0, 200 - patient.heartRate) / 100) * 20 +
        (Math.max(0, 140 - patient.bloodPressureSystolic) / 100) * 20
      )

      difficultyManager.updatePerformance({
        missionsCompleted: stats.missionsCompleted,
        totalMissions: stats.missionsCompleted + (gameStatus === 'lost' ? 1 : 0),
        avgResponseTime: timeRemaining,
        avgPatientHealth: overallHealth,
        avgTimeRemaining: timeRemaining
      })

      const newDifficulty = difficultyManager.adjustDifficulty()
      updateAIDifficulty(newDifficulty)
      updateAIPerformance(difficultyManager.getCurrentDifficulty() as any)
    }

    updatePerformance()
  }, [gameStatus, patient, stats, timeRemaining, updateAIDifficulty, updateAIPerformance])

  return null
}

export function enableVoiceControl() {
  if (voiceController && !voiceController.getIsListening()) {
    return voiceController.start()
  }
  return false
}

export function disableVoiceControl() {
  voiceController?.stop()
}

