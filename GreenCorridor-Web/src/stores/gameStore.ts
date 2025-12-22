import { create } from 'zustand'
import { PerformanceMetrics } from '../ai/DifficultyManager'

export interface PatientVitals {
  consciousness: number
  heartRate: number
  oxygenSaturation: number
  bloodPressureSystolic: number
  bloodPressureDiastolic: number
  isBleeding: boolean
  requiresCPR: boolean
  requiresOxygen: boolean
  bloodLossRate: number
  oxygenApplied: boolean
  bleedingControlled: boolean
  cprInProgress: boolean
}

export interface Mission {
  id: number
  name: string
  description: string
  patientCondition: string
  timeLimit: number
  difficulty: 'Easy' | 'Medium' | 'Hard' | 'Expert'
  startPosition: [number, number, number]
  hospitalPosition: [number, number, number]
  trafficDensity: number
  weather: 'Clear' | 'Rain' | 'Fog'
}

interface GameState {
  isPaused: boolean
  gameTime: number
  score: number
  timeRemaining: number
  goldenHour: number
  isEmergencyActive: boolean
  gameStatus: 'menu' | 'playing' | 'won' | 'lost' | 'paused'
  currentMission: Mission | null
  missionProgress: number
  
  ambulancePosition: [number, number, number]
  ambulanceSpeed: number
  ambulanceHeading: number
  distanceTraveled: number
  
  patient: PatientVitals
  patientName: string
  patientAge: number
  emergencyType: string
  timeSinceIncident: number
  timeToHospital: number
  
  trafficSignals: Array<{
    id: number
    position: [number, number, number]
    state: 'red' | 'yellow' | 'green' | 'emergency'
    preempted: boolean
  }>
  
  realtimeWeather: {
    temperature: number
    condition: string
    visibility: number
  } | null
  realtimeTraffic: {
    density: number
    flow: number
  } | null
  
  stats: {
    missionsCompleted: number
    totalScore: number
    bestTime: number
    emergencyActivations: number
    signalsPreempted: number
  }
  
  aiDifficulty: GameDifficulty | null
  aiPerformance: PerformanceMetrics | null
  
  startMission: (mission: Mission) => void
  togglePause: () => void
  updateGameTime: (delta: number) => void
  updateAmbulance: (position: [number, number, number], speed: number, heading: number) => void
  updatePatient: (updates: Partial<PatientVitals>) => void
  activateEmergency: () => void
  addScore: (points: number) => void
  resetGame: () => void
  winGame: () => void
  loseGame: (reason: string) => void
  setGameStatus: (status: GameState['gameStatus']) => void
  updateAIDifficulty: (difficulty: GameDifficulty) => void
  updateAIPerformance: (metrics: Partial<PerformanceMetrics>) => void
}

interface GameDifficulty {
  trafficDensity: number
  timeLimitMultiplier: number
  patientDeteriorationRate: number
  trafficSignalCooldown: number
  npcAwareness: number
  weatherSeverity: number
}

const initialPatient: PatientVitals = {
  consciousness: 100,
  heartRate: 80,
  oxygenSaturation: 98,
  bloodPressureSystolic: 120,
  bloodPressureDiastolic: 80,
  isBleeding: false,
  requiresCPR: false,
  requiresOxygen: false,
  bloodLossRate: 0,
  oxygenApplied: false,
  bleedingControlled: false,
  cprInProgress: false,
}

const defaultMissions: Mission[] = [
  {
    id: 1,
    name: 'Cardiac Emergency',
    description: 'Patient experiencing cardiac arrest. Get to the hospital immediately!',
    patientCondition: 'Cardiac Arrest',
    timeLimit: 600,
    difficulty: 'Easy',
    startPosition: [0, 0, 0],
    hospitalPosition: [100, 0, 100],
    trafficDensity: 3,
    weather: 'Clear',
  },
  {
    id: 2,
    name: 'Traumatic Injury',
    description: 'Severe bleeding detected. Time is critical!',
    patientCondition: 'Severe Bleeding',
    timeLimit: 480,
    difficulty: 'Medium',
    startPosition: [-50, 0, -50],
    hospitalPosition: [150, 0, 150],
    trafficDensity: 5,
    weather: 'Clear',
  },
  {
    id: 3,
    name: 'Respiratory Failure',
    description: 'Patient cannot breathe. Oxygen levels dropping rapidly!',
    patientCondition: 'Respiratory Failure',
    timeLimit: 420,
    difficulty: 'Hard',
    startPosition: [0, 0, 0],
    hospitalPosition: [200, 0, 200],
    trafficDensity: 7,
    weather: 'Rain',
  },
  {
    id: 4,
    name: 'Multi-System Failure',
    description: 'Critical patient with multiple organ failure. Extreme urgency!',
    patientCondition: 'Multi-System Failure',
    timeLimit: 360,
    difficulty: 'Expert',
    startPosition: [-100, 0, -100],
    hospitalPosition: [250, 0, 250],
    trafficDensity: 10,
    weather: 'Fog',
  },
]

export const useGameStore = create<GameState>((set, get) => ({
  // Initial state
  isPaused: false,
  gameTime: 0,
  score: 0,
  timeRemaining: 600,
  goldenHour: 600,
  isEmergencyActive: false,
  gameStatus: 'menu',
  currentMission: null,
  missionProgress: 0,
  ambulancePosition: [0, 0, 0],
  ambulanceSpeed: 0,
  ambulanceHeading: 0,
  distanceTraveled: 0,
  patient: initialPatient,
  patientName: 'John Doe',
  patientAge: 45,
  emergencyType: 'Cardiac Arrest',
  timeSinceIncident: 0,
  timeToHospital: 0,
  trafficSignals: [],
  realtimeWeather: null,
  realtimeTraffic: null,
  stats: {
    missionsCompleted: 0,
    totalScore: 0,
    bestTime: 0,
    emergencyActivations: 0,
    signalsPreempted: 0,
  },
  aiDifficulty: null,
  aiPerformance: null,
  
  // Actions
  startMission: (mission) => {
    // Initialize patient based on mission
    let patient: PatientVitals = { ...initialPatient }
    
    switch (mission.patientCondition) {
      case 'Cardiac Arrest':
        patient.heartRate = 30
        patient.consciousness = 20
        patient.requiresCPR = true
        break
      case 'Severe Bleeding':
        patient.isBleeding = true
        patient.bloodLossRate = 2
        patient.consciousness = 40
        break
      case 'Respiratory Failure':
        patient.oxygenSaturation = 65
        patient.requiresOxygen = true
        patient.consciousness = 50
        break
      case 'Multi-System Failure':
        patient.heartRate = 40
        patient.oxygenSaturation = 70
        patient.isBleeding = true
        patient.bloodLossRate = 1.5
        patient.consciousness = 30
        patient.requiresCPR = true
        patient.requiresOxygen = true
        break
    }
    
    set({
      gameStatus: 'playing',
      currentMission: mission,
      timeRemaining: mission.timeLimit,
      goldenHour: mission.timeLimit,
      ambulancePosition: mission.startPosition,
      ambulanceSpeed: 0,
      ambulanceHeading: 0,
      distanceTraveled: 0,
      patient,
      emergencyType: mission.patientCondition,
      patientName: `Patient ${mission.id}`,
      timeSinceIncident: 0,
      score: 0,
      gameTime: 0,
      isPaused: false,
      isEmergencyActive: false,
      missionProgress: 0,
    })
  },
  
  togglePause: () => {
    const state = get()
    if (state.gameStatus === 'playing') {
      set({ isPaused: !state.isPaused, gameStatus: state.isPaused ? 'playing' : 'paused' })
    }
  },
  
  updateGameTime: (delta: number) => {
    const state = get()
    if (!state.isPaused && state.gameStatus === 'playing') {
      const newTimeRemaining = Math.max(0, state.timeRemaining - delta)
      const newGameTime = state.gameTime + delta
      
      const patient = { ...state.patient }
      let healthChanged = false
      
      if (patient.isBleeding && !patient.bleedingControlled) {
        patient.consciousness = Math.max(0, patient.consciousness - delta * 0.15)
        healthChanged = true
      }
      if (patient.requiresOxygen && !patient.oxygenApplied) {
        patient.oxygenSaturation = Math.max(0, patient.oxygenSaturation - delta * 0.25)
        healthChanged = true
      }
      if (patient.requiresCPR && !patient.cprInProgress) {
        patient.heartRate = Math.max(0, patient.heartRate - delta * 0.6)
        healthChanged = true
      }
      
      const hospitalPos = state.currentMission?.hospitalPosition || [100, 0, 100]
      const ambulancePos = state.ambulancePosition
      const distanceToHospital = Math.sqrt(
        Math.pow(hospitalPos[0] - ambulancePos[0], 2) +
        Math.pow(hospitalPos[2] - ambulancePos[2], 2)
      )
      const totalDistance = Math.sqrt(
        Math.pow(hospitalPos[0] - (state.currentMission?.startPosition[0] || 0), 2) +
        Math.pow(hospitalPos[2] - (state.currentMission?.startPosition[2] || 0), 2)
      )
      const progress = Math.max(0, Math.min(100, ((totalDistance - distanceToHospital) / totalDistance) * 100))
      
      if (distanceToHospital < 10 && state.ambulanceSpeed < 5) {
        get().winGame()
        return
      }
      
      if (newTimeRemaining <= 0) {
        get().loseGame('Time expired! Golden Hour is over.')
        return
      }
      
      const overallHealth = Math.round(
        patient.consciousness * 0.3 +
        (patient.oxygenSaturation / 100) * 30 +
        (Math.max(0, 200 - patient.heartRate) / 100) * 20 +
        (Math.max(0, 140 - patient.bloodPressureSystolic) / 100) * 20
      )
      
      if (overallHealth <= 0) {
        get().loseGame('Patient health dropped to zero!')
        return
      }
      
      set({
        gameTime: newGameTime,
        timeRemaining: newTimeRemaining,
        timeSinceIncident: state.timeSinceIncident + delta,
        patient: healthChanged ? patient : state.patient,
        missionProgress: progress,
      })
    }
  },
  
  updateAmbulance: (position, speed, heading) => {
    const state = get()
    const lastPos = state.ambulancePosition
    const distance = Math.sqrt(
      Math.pow(position[0] - lastPos[0], 2) +
      Math.pow(position[2] - lastPos[2], 2)
    )
    
    set({
      ambulancePosition: position,
      ambulanceSpeed: speed,
      ambulanceHeading: heading,
      distanceTraveled: state.distanceTraveled + distance,
    })
  },
  
  updatePatient: (updates) => {
    set((state) => ({
      patient: { ...state.patient, ...updates },
    }))
  },
  
  activateEmergency: () => {
    const state = get()
    if (!state.isEmergencyActive) {
      set({ isEmergencyActive: true })
      get().addScore(500)
      set((s) => ({
        stats: {
          ...s.stats,
          emergencyActivations: s.stats.emergencyActivations + 1,
        },
      }))
    }
  },
  
  addScore: (points) => {
    set((state) => ({ score: state.score + points }))
  },
  
  winGame: () => {
    const state = get()
    const patient = state.patient
    const overallHealth = Math.round(
      patient.consciousness * 0.3 +
      (patient.oxygenSaturation / 100) * 30 +
      (Math.max(0, 200 - patient.heartRate) / 100) * 20 +
      (Math.max(0, 140 - patient.bloodPressureSystolic) / 100) * 20
    )
    const timeBonus = Math.floor(state.timeRemaining * 10)
    const healthBonus = Math.floor(overallHealth * 10)
    const finalScore = state.score + 1000 + timeBonus + healthBonus
    
    set({
      gameStatus: 'won',
      score: finalScore,
      stats: {
        missionsCompleted: state.stats.missionsCompleted + 1,
        totalScore: state.stats.totalScore + finalScore,
        bestTime: state.stats.bestTime === 0 || state.gameTime < state.stats.bestTime
          ? state.gameTime
          : state.stats.bestTime,
        emergencyActivations: state.stats.emergencyActivations,
        signalsPreempted: state.stats.signalsPreempted,
      },
    })
  },
  
  loseGame: (reason: string) => {
    set({ gameStatus: 'lost' })
    console.log(`Game Over: ${reason}`)
  },
  
  resetGame: () => {
    set({
      isPaused: false,
      gameTime: 0,
      score: 0,
      timeRemaining: 600,
      isEmergencyActive: false,
      ambulancePosition: [0, 0, 0],
      ambulanceSpeed: 0,
      ambulanceHeading: 0,
      distanceTraveled: 0,
      patient: initialPatient,
      timeSinceIncident: 0,
      timeToHospital: 0,
      missionProgress: 0,
    })
  },
  
  setGameStatus: (status) => {
    set({ gameStatus: status })
  },
  
  updateAIDifficulty: (difficulty) => {
    set({ aiDifficulty: difficulty })
  },
  
  updateAIPerformance: (metrics) => {
    set((state) => ({
      aiPerformance: state.aiPerformance 
        ? { ...state.aiPerformance, ...metrics }
        : {
            successRate: 0.5,
            avgResponseTime: 60,
            missionsCompleted: 0,
            totalMissions: 0,
            avgPatientHealth: 50,
            avgTimeRemaining: 30,
            ...metrics
          }
    }))
  },
}))

export const missions = defaultMissions
