import * as tf from '@tensorflow/tfjs'

export interface TrafficFeatures {
  timeOfDay: number
  dayOfWeek: number
  weather: number
  areaType: number
  specialEvent: number
}

export class TrafficPredictor {
  private model: tf.LayersModel | null = null
  private isInitialized: boolean = false

  async initialize() {
    if (this.isInitialized) return

    this.model = tf.sequential({
      layers: [
        tf.layers.dense({ units: 16, activation: 'relu', inputShape: [5] }),
        tf.layers.dropout({ rate: 0.2 }),
        tf.layers.dense({ units: 8, activation: 'relu' }),
        tf.layers.dense({ units: 1, activation: 'sigmoid' })
      ]
    })

    this.model.compile({
      optimizer: 'adam',
      loss: 'meanSquaredError',
      metrics: ['accuracy']
    })

    await this.trainOnSyntheticData()
    this.isInitialized = true
  }

  async predictTraffic(features: TrafficFeatures): Promise<number> {
    if (!this.model) {
      await this.initialize()
    }

    const input = tf.tensor2d([this.featureToArray(features)])
    const prediction = this.model!.predict(input) as tf.Tensor
    const value = await prediction.data()
    input.dispose()
    prediction.dispose()
    
    return Math.max(0, Math.min(1, value[0]))
  }

  private featureToArray(features: TrafficFeatures): number[] {
    return [
      features.timeOfDay / 24,
      features.dayOfWeek / 7,
      features.weather,
      features.areaType,
      features.specialEvent
    ]
  }

  private async trainOnSyntheticData() {
    if (!this.model) return

    const { features, labels } = this.generateTrainingData()

    await this.model.fit(features, labels, {
      epochs: 50,
      batchSize: 32,
      verbose: 0,
      shuffle: true
    })

    features.dispose()
    labels.dispose()
  }

  private generateTrainingData(): { features: tf.Tensor2D; labels: tf.Tensor2D } {
    const samples: number[][] = []
    const targets: number[] = []

    for (let i = 0; i < 1000; i++) {
      const timeOfDay = Math.random() * 24
      const dayOfWeek = Math.floor(Math.random() * 7)
      const weather = Math.random()
      const areaType = Math.random()
      const specialEvent = Math.random() > 0.9 ? 1 : 0

      const isRushHour = (timeOfDay >= 7 && timeOfDay <= 9) || (timeOfDay >= 17 && timeOfDay <= 19)
      const isWeekend = dayOfWeek === 0 || dayOfWeek === 6

      let trafficDensity = 0.3
      if (isRushHour) trafficDensity += 0.4
      if (!isWeekend) trafficDensity += 0.2
      if (weather < 0.3) trafficDensity += 0.1
      if (specialEvent) trafficDensity += 0.2
      trafficDensity += areaType * 0.1

      samples.push([timeOfDay / 24, dayOfWeek / 7, weather, areaType, specialEvent])
      targets.push(Math.max(0, Math.min(1, trafficDensity)))
    }

    return {
      features: tf.tensor2d(samples),
      labels: tf.tensor2d(targets, [targets.length, 1])
    }
  }

  async dispose() {
    if (this.model) {
      this.model.dispose()
      this.model = null
      this.isInitialized = false
    }
  }
}

