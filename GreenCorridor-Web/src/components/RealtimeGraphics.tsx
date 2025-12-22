import { useEffect, useState, useRef } from 'react'
import { useGameStore } from '../stores/gameStore'
import { realtimeAPI, WeatherData, TrafficData, GPSData } from '../services/RealtimeAPI'
import './RealtimeGraphics.css'

export default function RealtimeGraphics() {
  const { ambulancePosition, currentMission } = useGameStore()
  const [weather, setWeather] = useState<WeatherData | null>(null)
  const [traffic, setTraffic] = useState<TrafficData | null>(null)
  const [realGPS, setRealGPS] = useState<GPSData | null>(null)
  const [mapTiles, setMapTiles] = useState<string[]>([])
  const weatherIntervalRef = useRef<number | null>(null)
  const trafficIntervalRef = useRef<number | null>(null)
  const gpsWatchIdRef = useRef<number | null>(null)

  useEffect(() => {
    const initRealtimeData = async () => {
      const lat = currentMission?.hospitalPosition[0] ? 
        (currentMission.hospitalPosition[0] / 111000) + 13.0827 : 13.0827
      const lon = currentMission?.hospitalPosition[2] ? 
        (currentMission.hospitalPosition[2] / 111000) + 80.2707 : 80.2707

      const weatherData = await realtimeAPI.getWeatherData(lat, lon)
      setWeather(weatherData)

      const trafficData = await realtimeAPI.getTrafficData(lat, lon)
      setTraffic(trafficData)

      const gps = await realtimeAPI.getRealGPS()
      if (gps) {
        setRealGPS(gps)
      }

      const watchId = realtimeAPI.watchGPS((data) => {
        setRealGPS(data)
      })
      if (watchId !== null) {
        gpsWatchIdRef.current = watchId
      }

      const tiles: string[] = []
      for (let dx = -1; dx <= 1; dx++) {
        for (let dy = -1; dy <= 1; dy++) {
          tiles.push(realtimeAPI.getMapTileUrl(lat + dx * 0.01, lon + dy * 0.01, 15))
        }
      }
      setMapTiles(tiles)
    }

    if (currentMission) {
      initRealtimeData()
    }

    weatherIntervalRef.current = window.setInterval(async () => {
      const lat = realGPS?.latitude || 13.0827
      const lon = realGPS?.longitude || 80.2707
      const weatherData = await realtimeAPI.getWeatherData(lat, lon)
      setWeather(weatherData)
    }, 300000)

    trafficIntervalRef.current = window.setInterval(async () => {
      const lat = realGPS?.latitude || 13.0827
      const lon = realGPS?.longitude || 80.2707
      const trafficData = await realtimeAPI.getTrafficData(lat, lon)
      setTraffic(trafficData)
    }, 60000)

    return () => {
      if (weatherIntervalRef.current) clearInterval(weatherIntervalRef.current)
      if (trafficIntervalRef.current) clearInterval(trafficIntervalRef.current)
      if (gpsWatchIdRef.current !== null) {
        navigator.geolocation.clearWatch(gpsWatchIdRef.current)
      }
    }
  }, [currentMission, realGPS])

  useEffect(() => {
    if (!weather) return

    const root = document.documentElement
    root.style.setProperty('--weather-visibility', `${weather.visibility}km`)
    root.style.setProperty('--weather-condition', weather.condition.toLowerCase())
    
    document.body.className = document.body.className.replace(/weather-\w+/g, '')
    document.body.classList.add(`weather-${weather.condition.toLowerCase()}`)
  }, [weather])

  useEffect(() => {
    if (!traffic) return

    const root = document.documentElement
    root.style.setProperty('--traffic-density', `${traffic.density}%`)
    root.style.setProperty('--traffic-flow', `${traffic.flow}%`)
  }, [traffic])

  if (!weather && !traffic) return null

  return (
    <div className="realtime-graphics-overlay">
      {weather && (
        <div className="weather-effects">
          {weather.condition === 'Rain' && (
            <div className="rain-effect">
              {Array.from({ length: 100 }).map((_, i) => (
                <div
                  key={i}
                  className="rain-drop"
                  style={{
                    left: `${Math.random() * 100}%`,
                    animationDelay: `${Math.random() * 2}s`,
                    animationDuration: `${0.5 + Math.random() * 0.5}s`,
                  }}
                />
              ))}
            </div>
          )}
          {weather.condition === 'Fog' && (
            <div className="fog-effect">
              {Array.from({ length: 5 }).map((_, i) => (
                <div
                  key={i}
                  className="fog-layer"
                  style={{
                    opacity: 0.3 + Math.random() * 0.2,
                    animationDelay: `${i * 2}s`,
                  }}
                />
              ))}
            </div>
          )}
          {weather.condition === 'Storm' && (
            <>
              <div className="lightning-effect" />
              <div className="rain-effect">
                {Array.from({ length: 150 }).map((_, i) => (
                  <div
                    key={i}
                    className="rain-drop heavy"
                    style={{
                      left: `${Math.random() * 100}%`,
                      animationDelay: `${Math.random() * 1}s`,
                      animationDuration: `${0.3 + Math.random() * 0.3}s`,
                    }}
                  />
                ))}
              </div>
            </>
          )}
        </div>
      )}

      {process.env.NODE_ENV === 'development' && (
        <div className="realtime-debug-panel">
          {weather && (
            <div className="debug-item">
              <span>Weather:</span>
              <span>{weather.condition} {weather.temperature.toFixed(1)}°C</span>
            </div>
          )}
          {traffic && (
            <div className="debug-item">
              <span>Traffic:</span>
              <span>{traffic.density.toFixed(0)}% density</span>
            </div>
          )}
          {realGPS && (
            <div className="debug-item">
              <span>GPS:</span>
              <span>{realGPS.latitude.toFixed(5)}, {realGPS.longitude.toFixed(5)}</span>
            </div>
          )}
        </div>
      )}
    </div>
  )
}


