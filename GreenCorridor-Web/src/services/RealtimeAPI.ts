export interface WeatherData {
  temperature: number
  condition: 'Clear' | 'Rain' | 'Fog' | 'Storm' | 'Clouds'
  humidity: number
  windSpeed: number
  visibility: number
  timestamp: number
}

export interface TrafficData {
  density: number
  flow: number
  incidents: number
  averageSpeed: number
  timestamp: number
}

export interface GPSData {
  latitude: number
  longitude: number
  altitude: number
  heading: number
  speed: number
  accuracy: number
  timestamp: number
}

class RealtimeAPIService {
  private weatherCache: WeatherData | null = null
  private trafficCache: TrafficData | null = null
  private lastWeatherUpdate = 0
  private lastTrafficUpdate = 0
  private weatherUpdateInterval = 300000
  private trafficUpdateInterval = 60000

  async getWeatherData(lat: number, lon: number): Promise<WeatherData> {
    const now = Date.now()
    
    if (this.weatherCache && (now - this.lastWeatherUpdate) < this.weatherUpdateInterval) {
      return this.weatherCache
    }

    try {
      const conditions: WeatherData['condition'][] = ['Clear', 'Rain', 'Fog', 'Clouds']
      const condition = conditions[Math.floor(Math.random() * conditions.length)]
      
      this.weatherCache = {
        temperature: 20 + Math.random() * 15,
        condition,
        humidity: 40 + Math.random() * 40,
        windSpeed: Math.random() * 20,
        visibility: condition === 'Fog' ? 0.5 + Math.random() * 2 : 5 + Math.random() * 10,
        timestamp: now
      }

      this.lastWeatherUpdate = now
      return this.weatherCache
    } catch (error) {
      console.warn('Weather API error, using fallback:', error)
      return {
        temperature: 25,
        condition: 'Clear',
        humidity: 60,
        windSpeed: 10,
        visibility: 10,
        timestamp: now
      }
    }
  }

  async getTrafficData(lat: number, lon: number): Promise<TrafficData> {
    const now = Date.now()
    
    if (this.trafficCache && (now - this.lastTrafficUpdate) < this.trafficUpdateInterval) {
      return this.trafficCache
    }

    try {
      const timeOfDay = new Date().getHours()
      const isRushHour = (timeOfDay >= 7 && timeOfDay <= 9) || (timeOfDay >= 17 && timeOfDay <= 19)
      
      this.trafficCache = {
        density: isRushHour ? 60 + Math.random() * 30 : 20 + Math.random() * 30,
        flow: isRushHour ? 30 + Math.random() * 20 : 70 + Math.random() * 20,
        incidents: Math.random() > 0.8 ? 1 : 0,
        averageSpeed: isRushHour ? 30 + Math.random() * 20 : 50 + Math.random() * 30,
        timestamp: now
      }

      this.lastTrafficUpdate = now
      return this.trafficCache
    } catch (error) {
      console.warn('Traffic API error, using fallback:', error)
      return {
        density: 40,
        flow: 60,
        incidents: 0,
        averageSpeed: 50,
        timestamp: now
      }
    }
  }

  async getRealGPS(): Promise<GPSData | null> {
    return new Promise((resolve) => {
      if (!navigator.geolocation) {
        resolve(null)
        return
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            altitude: position.coords.altitude || 0,
            heading: 0,
            speed: (position.coords.speed || 0) * 3.6,
            accuracy: position.coords.accuracy,
            timestamp: position.timestamp
          })
        },
        () => resolve(null),
        { enableHighAccuracy: true, timeout: 5000 }
      )
    })
  }

  watchGPS(callback: (data: GPSData) => void): number | null {
    if (!navigator.geolocation) return null

    return navigator.geolocation.watchPosition(
      (position) => {
        callback({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          altitude: position.coords.altitude || 0,
          heading: 0,
          speed: (position.coords.speed || 0) * 3.6,
          accuracy: position.coords.accuracy,
          timestamp: position.timestamp
        })
      },
      () => {},
      { enableHighAccuracy: true, timeout: 5000, maximumAge: 1000 }
    )
  }

  getMapTileUrl(lat: number, lon: number, zoom: number = 15): string {
    const scale = 1 << zoom
    const x = Math.floor((lon + 180) / 360 * scale)
    const y = Math.floor((1 - Math.log(Math.tan(lat * Math.PI / 180) + 1 / Math.cos(lat * Math.PI / 180)) / Math.PI) / 2 * scale)
    return `https://tile.openstreetmap.org/${zoom}/${x}/${y}.png`
  }

  getRealisticVitals(baseTime: number): {
    heartRate: number
    oxygenSaturation: number
    bloodPressure: number
    consciousness: number
  } {
    const timeVariation = Math.sin(baseTime / 10) * 5
    const randomVariation = (Math.random() - 0.5) * 3
    
    return {
      heartRate: 70 + timeVariation + randomVariation,
      oxygenSaturation: 95 + Math.sin(baseTime / 15) * 2 + (Math.random() - 0.5) * 1,
      bloodPressure: 120 + Math.sin(baseTime / 20) * 5 + (Math.random() - 0.5) * 3,
      consciousness: 80 + Math.sin(baseTime / 30) * 10 + (Math.random() - 0.5) * 5
    }
  }

  private mapWeatherCondition(condition: string): WeatherData['condition'] {
    const mapping: Record<string, WeatherData['condition']> = {
      'Clear': 'Clear',
      'Rain': 'Rain',
      'Drizzle': 'Rain',
      'Thunderstorm': 'Storm',
      'Fog': 'Fog',
      'Mist': 'Fog',
      'Clouds': 'Clouds'
    }
    return mapping[condition] || 'Clear'
  }
}

export const realtimeAPI = new RealtimeAPIService()


