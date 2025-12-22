import { useEffect, useState } from 'react'
import { MapContainer, TileLayer, Marker, Popup, Polyline, useMap } from 'react-leaflet'
import L from 'leaflet'
import { useGameStore } from '../stores/gameStore'
import { realtimeAPI } from '../services/RealtimeAPI'
import 'leaflet/dist/leaflet.css'
import './LeafletMap.css'

delete (L.Icon.Default.prototype as any)._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
})

const ambulanceIcon = L.icon({
  iconUrl: 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(`
    <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32">
      <rect width="32" height="32" fill="#FF0000" rx="4"/>
      <circle cx="16" cy="16" r="8" fill="white"/>
      <path d="M12 12h8v8h-8z" fill="#FF0000"/>
      <path d="M14 14h4v4h-4z" fill="white"/>
    </svg>
  `),
  iconSize: [32, 32],
  iconAnchor: [16, 16],
  popupAnchor: [0, -16],
})

const hospitalIcon = L.icon({
  iconUrl: 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(`
    <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32">
      <rect width="32" height="32" fill="#00AA00" rx="4"/>
      <rect x="12" y="8" width="8" height="16" fill="white"/>
      <rect x="14" y="10" width="4" height="12" fill="#00AA00"/>
      <line x1="16" y1="10" x2="16" y2="22" stroke="white" stroke-width="2"/>
      <line x1="12" y1="16" x2="20" y2="16" stroke="white" stroke-width="2"/>
    </svg>
  `),
  iconSize: [32, 32],
  iconAnchor: [16, 16],
  popupAnchor: [0, -16],
})

function MapUpdater() {
  const map = useMap()
  const { ambulancePosition } = useGameStore()
  const [realGPS, setRealGPS] = useState<{ lat: number; lon: number } | null>(null)

  useEffect(() => {
    realtimeAPI.getRealGPS().then((gps) => {
      if (gps) {
        setRealGPS({ lat: gps.latitude, lon: gps.longitude })
        map.setView([gps.latitude, gps.longitude], 15)
      } else {
        const lat = 13.0827 + (ambulancePosition[0] / 111000)
        const lon = 80.2707 + (ambulancePosition[2] / 111000)
        map.setView([lat, lon], 15)
      }
    })

    const watchId = realtimeAPI.watchGPS((gps) => {
      setRealGPS({ lat: gps.latitude, lon: gps.longitude })
      map.setView([gps.latitude, gps.longitude], 15)
    })

    return () => {
      if (watchId !== null) {
        navigator.geolocation.clearWatch(watchId)
      }
    }
  }, [map])

  useEffect(() => {
    if (realGPS) {
      map.setView([realGPS.lat, realGPS.lon], 15, { animate: true, duration: 0.5 })
    } else if (ambulancePosition) {
      const lat = 13.0827 + (ambulancePosition[0] / 111000)
      const lon = 80.2707 + (ambulancePosition[2] / 111000)
      map.setView([lat, lon], 15, { animate: true, duration: 0.5 })
    }
  }, [ambulancePosition, realGPS, map])

  return null
}

export default function LeafletMap() {
  const { ambulancePosition, currentMission, ambulanceSpeed, ambulanceHeading } = useGameStore()
  const [routePath, setRoutePath] = useState<[number, number][]>([])
  const [realGPS, setRealGPS] = useState<{ lat: number; lon: number } | null>(null)

  const gameToLatLon = (x: number, z: number): [number, number] => {
    return [13.0827 + (x / 111000), 80.2707 + (z / 111000)]
  }

  const ambulanceLatLon: [number, number] = realGPS 
    ? [realGPS.lat, realGPS.lon]
    : gameToLatLon(ambulancePosition[0], ambulancePosition[2])

  const hospitalLatLon = currentMission
    ? gameToLatLon(currentMission.hospitalPosition[0], currentMission.hospitalPosition[2])
    : null

  useEffect(() => {
    if (hospitalLatLon) {
      setRoutePath([ambulanceLatLon, hospitalLatLon])
    }
  }, [ambulanceLatLon, hospitalLatLon])

  useEffect(() => {
    realtimeAPI.getRealGPS().then((gps) => {
      if (gps) {
        setRealGPS({ lat: gps.latitude, lon: gps.longitude })
      }
    })

    const watchId = realtimeAPI.watchGPS((gps) => {
      setRealGPS({ lat: gps.latitude, lon: gps.longitude })
    })

    return () => {
      if (watchId !== null) {
        navigator.geolocation.clearWatch(watchId)
      }
    }
  }, [])

  const distanceToHospital = hospitalLatLon
    ? L.latLng(ambulanceLatLon).distanceTo(L.latLng(hospitalLatLon))
    : 0

  return (
    <div className="leaflet-map-container">
      <MapContainer
        center={ambulanceLatLon}
        zoom={15}
        style={{ height: '100%', width: '100%' }}
        zoomControl={true}
        scrollWheelZoom={true}
      >
        <MapUpdater />
        
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          maxZoom={19}
        />

        {routePath.length > 0 && (
          <Polyline
            positions={routePath}
            color="#FF0000"
            weight={4}
            opacity={0.7}
            dashArray="10, 10"
          />
        )}

        <Marker
          position={ambulanceLatLon}
          icon={ambulanceIcon}
        >
          <Popup>
            <div className="map-popup">
              <strong>🚑 Ambulance</strong>
              <p>Speed: {Math.round(ambulanceSpeed)} km/h</p>
              <p>Heading: {Math.round(ambulanceHeading * (180 / Math.PI))}°</p>
              {realGPS && <p>📍 Real GPS: Active</p>}
            </div>
          </Popup>
        </Marker>

        {hospitalLatLon && (
          <Marker position={hospitalLatLon} icon={hospitalIcon}>
            <Popup>
              <div className="map-popup">
                <strong>🏥 Hospital</strong>
                <p>Distance: {distanceToHospital > 1000 
                  ? `${(distanceToHospital / 1000).toFixed(2)} km`
                  : `${Math.round(distanceToHospital)} m`}</p>
                {currentMission && (
                  <>
                    <p>Mission: {currentMission.name}</p>
                    <p>Patient: {currentMission.patientCondition}</p>
                  </>
                )}
              </div>
            </Popup>
          </Marker>
        )}
      </MapContainer>

      <div className="map-controls-overlay">
        <div className="map-info-panel">
          <div className="map-info-item">
            <span className="map-info-label">Distance:</span>
            <span className="map-info-value">
              {distanceToHospital > 1000
                ? `${(distanceToHospital / 1000).toFixed(2)} km`
                : `${Math.round(distanceToHospital)} m`}
            </span>
          </div>
          <div className="map-info-item">
            <span className="map-info-label">Speed:</span>
            <span className="map-info-value">{Math.round(ambulanceSpeed)} km/h</span>
          </div>
          {realGPS && (
            <div className="map-info-item">
              <span className="map-info-label">GPS:</span>
              <span className="map-info-value">📍 Active</span>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

