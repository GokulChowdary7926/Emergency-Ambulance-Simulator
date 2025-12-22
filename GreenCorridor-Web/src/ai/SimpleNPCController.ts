export type NPCAction = 'pull_over' | 'slow_down' | 'continue' | 'panic' | 'normal'

export interface NPCState {
  position: [number, number, number]
  speed: number
  awareness: number
  behavior: NPCAction
}

export class SmartNPC {
  private awareness: number
  private reactionTime: number

  constructor(awareness: number = 0.7, reactionTime: number = 1.0) {
    this.awareness = awareness
    this.reactionTime = reactionTime
  }

  makeDecision(
    ambulanceNearby: boolean,
    ambulanceDistance: number,
    trafficDensity: number,
    isEmergencyActive: boolean
  ): NPCAction {
    if (!ambulanceNearby) {
      return this.normalDrivingBehavior(trafficDensity)
    }

    if (isEmergencyActive && ambulanceDistance < 50) {
      return this.selectWithUncertainty([
        { action: 'pull_over', weight: 0.85 },
        { action: 'slow_down', weight: 0.10 },
        { action: 'panic', weight: 0.05 }
      ])
    }

    if (isEmergencyActive && ambulanceDistance < 100) {
      return this.selectWithUncertainty([
        { action: 'slow_down', weight: 0.70 },
        { action: 'pull_over', weight: 0.25 },
        { action: 'continue', weight: 0.05 }
      ])
    }

    if (ambulanceDistance < 150) {
      return this.selectWithUncertainty([
        { action: 'slow_down', weight: 0.60 },
        { action: 'continue', weight: 0.35 },
        { action: 'pull_over', weight: 0.05 }
      ])
    }

    return 'normal'
  }

  private normalDrivingBehavior(trafficDensity: number): NPCAction {
    if (trafficDensity > 0.7) {
      return this.selectWithUncertainty([
        { action: 'slow_down', weight: 0.80 },
        { action: 'continue', weight: 0.20 }
      ])
    }

    if (trafficDensity > 0.4) {
      return this.selectWithUncertainty([
        { action: 'continue', weight: 0.70 },
        { action: 'slow_down', weight: 0.30 }
      ])
    }

    return 'normal'
  }

  private selectWithUncertainty(actions: Array<{ action: NPCAction; weight: number }>): NPCAction {
    const totalWeight = actions.reduce((sum, a) => sum + a.weight, 0)
    let random = Math.random() * totalWeight

    for (const { action, weight } of actions) {
      random -= weight
      if (random <= 0) {
        if (Math.random() > this.awareness) {
          return 'continue'
        }
        return action
      }
    }

    return actions[0].action
  }

  updateAwareness(success: boolean) {
    if (success) {
      this.awareness = Math.min(1.0, this.awareness + 0.01)
    } else {
      this.awareness = Math.max(0.3, this.awareness - 0.01)
    }
  }

  getReactionDelay(): number {
    return this.reactionTime * (1 + (1 - this.awareness))
  }
}

