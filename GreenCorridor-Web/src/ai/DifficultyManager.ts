export interface PerformanceMetrics {
  successRate: number
  avgResponseTime: number
  missionsCompleted: number
  totalMissions: number
  avgPatientHealth: number
  avgTimeRemaining: number
}

export interface GameDifficulty {
  trafficDensity: number
  timeLimitMultiplier: number
  patientDeteriorationRate: number
  trafficSignalCooldown: number
  npcAwareness: number
  weatherSeverity: number
}

export class AdaptiveDifficulty {
  private playerPerformance: PerformanceMetrics
  private currentDifficulty: GameDifficulty
  private difficultyHistory: GameDifficulty[]

  constructor() {
    this.playerPerformance = {
      successRate: 0.5,
      avgResponseTime: 60,
      missionsCompleted: 0,
      totalMissions: 0,
      avgPatientHealth: 50,
      avgTimeRemaining: 30
    }

    this.currentDifficulty = this.getDefaultDifficulty()
    this.difficultyHistory = [this.currentDifficulty]
  }

  updatePerformance(metrics: Partial<PerformanceMetrics>) {
    this.playerPerformance = { ...this.playerPerformance, ...metrics }
    
    if (this.playerPerformance.totalMissions > 0) {
      this.playerPerformance.successRate = 
        this.playerPerformance.missionsCompleted / this.playerPerformance.totalMissions
    }
  }

  adjustDifficulty(): GameDifficulty {
    const { successRate, avgResponseTime, avgPatientHealth, avgTimeRemaining } = this.playerPerformance

    if (successRate > 0.8 && avgResponseTime < 30 && avgPatientHealth > 70) {
      this.currentDifficulty = this.increaseDifficulty()
    } else if (successRate < 0.4 || avgResponseTime > 90 || avgPatientHealth < 30) {
      this.currentDifficulty = this.decreaseDifficulty()
    } else {
      this.currentDifficulty = this.maintainDifficulty()
    }

    this.difficultyHistory.push({ ...this.currentDifficulty })
    return this.currentDifficulty
  }

  private increaseDifficulty(): GameDifficulty {
    return {
      trafficDensity: Math.min(0.9, this.currentDifficulty.trafficDensity + 0.1),
      timeLimitMultiplier: Math.max(0.7, this.currentDifficulty.timeLimitMultiplier - 0.1),
      patientDeteriorationRate: Math.min(1.5, this.currentDifficulty.patientDeteriorationRate + 0.1),
      trafficSignalCooldown: Math.max(5, this.currentDifficulty.trafficSignalCooldown - 2),
      npcAwareness: Math.min(0.9, this.currentDifficulty.npcAwareness + 0.1),
      weatherSeverity: Math.min(0.8, this.currentDifficulty.weatherSeverity + 0.1)
    }
  }

  private decreaseDifficulty(): GameDifficulty {
    return {
      trafficDensity: Math.max(0.2, this.currentDifficulty.trafficDensity - 0.1),
      timeLimitMultiplier: Math.min(1.3, this.currentDifficulty.timeLimitMultiplier + 0.1),
      patientDeteriorationRate: Math.max(0.5, this.currentDifficulty.patientDeteriorationRate - 0.1),
      trafficSignalCooldown: Math.min(15, this.currentDifficulty.trafficSignalCooldown + 2),
      npcAwareness: Math.max(0.4, this.currentDifficulty.npcAwareness - 0.1),
      weatherSeverity: Math.max(0.2, this.currentDifficulty.weatherSeverity - 0.1)
    }
  }

  private maintainDifficulty(): GameDifficulty {
    return { ...this.currentDifficulty }
  }

  private getDefaultDifficulty(): GameDifficulty {
    return {
      trafficDensity: 0.5,
      timeLimitMultiplier: 1.0,
      patientDeteriorationRate: 1.0,
      trafficSignalCooldown: 10,
      npcAwareness: 0.7,
      weatherSeverity: 0.5
    }
  }

  getCurrentDifficulty(): GameDifficulty {
    return { ...this.currentDifficulty }
  }

  reset() {
    this.currentDifficulty = this.getDefaultDifficulty()
    this.playerPerformance = {
      successRate: 0.5,
      avgResponseTime: 60,
      missionsCompleted: 0,
      totalMissions: 0,
      avgPatientHealth: 50,
      avgTimeRemaining: 30
    }
  }
}

