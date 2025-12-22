import { useState } from 'react'
import './HowToPlay.css'

interface HowToPlayProps {
  onClose: () => void
}

export default function HowToPlay({ onClose }: HowToPlayProps) {
  return (
    <div className="how-to-play-overlay" onClick={onClose}>
      <div className="how-to-play-modal" onClick={(e) => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>×</button>
        
        <div className="modal-header">
          <h1>🚑 Green Corridor</h1>
          <h2>How to Play</h2>
        </div>

        <div className="modal-content">
          <section className="section">
            <h3>🎯 Objective</h3>
            <p>
              Transport a critically ill patient to the hospital as quickly as possible
              while maintaining their health. You have a limited "Golden Hour" to complete the mission.
            </p>
          </section>

          <section className="section">
            <h3>🎮 Controls</h3>
            <div className="controls-grid">
              <div className="control-item">
                <kbd>W</kbd> or <kbd>↑</kbd>
                <span>Accelerate</span>
              </div>
              <div className="control-item">
                <kbd>S</kbd> or <kbd>↓</kbd>
                <span>Brake/Reverse</span>
              </div>
              <div className="control-item">
                <kbd>A</kbd> or <kbd>←</kbd>
                <span>Turn Left</span>
              </div>
              <div className="control-item">
                <kbd>D</kbd> or <kbd>→</kbd>
                <span>Turn Right</span>
              </div>
              <div className="control-item">
                <kbd>SPACE</kbd>
                <span>Emergency Mode (Lights & Siren)</span>
              </div>
              <div className="control-item">
                <kbd>Mouse</kbd>
                <span>Rotate Camera</span>
              </div>
              <div className="control-item">
                <kbd>Scroll</kbd>
                <span>Zoom In/Out</span>
              </div>
              <div className="control-item">
                <kbd>H</kbd>
                <span>Show/Hide Help</span>
              </div>
            </div>
          </section>

          <section className="section">
            <h3>🚦 Traffic Preemption System</h3>
            <ul>
              <li>When <strong>Emergency Mode</strong> is activated, traffic signals within <strong>250 meters</strong> automatically turn <strong>GREEN</strong></li>
              <li>This creates a "Green Corridor" for faster emergency response</li>
              <li>Use Emergency Mode strategically to clear intersections</li>
            </ul>
          </section>

          <section className="section">
            <h3>🏥 Patient Health System</h3>
            <ul>
              <li><strong>Monitor vitals:</strong> Heart Rate, Oxygen Saturation, Blood Pressure, Consciousness</li>
              <li><strong>Golden Hour:</strong> You have limited time to reach the hospital</li>
              <li><strong>Health Bar:</strong> Keep patient health above 30% to avoid critical condition</li>
              <li><strong>ETA:</strong> Estimated time to hospital is displayed when moving</li>
            </ul>
          </section>

          <section className="section">
            <h3>📊 Scoring System</h3>
            <ul>
              <li><strong>Time Bonus:</strong> Faster completion = higher score</li>
              <li><strong>Emergency Mode:</strong> +500 points when activated</li>
              <li><strong>Patient Health:</strong> Higher health at arrival = bonus points</li>
              <li><strong>Mission Success:</strong> +1000 base points for completing the mission</li>
            </ul>
          </section>

          <section className="section">
            <h3>🎯 Tips for Success</h3>
            <ol>
              <li>Activate Emergency Mode before approaching intersections</li>
              <li>Maintain speed but avoid crashes (they reduce patient health)</li>
              <li>Watch the timer - every second counts in the Golden Hour</li>
              <li>Look for the red hospital building with a red cross on top</li>
              <li>Use the GPS indicator to track your position</li>
              <li>Monitor patient vitals - critical health triggers warnings</li>
            </ol>
          </section>

          <section className="section">
            <h3>⚠️ Game Over Conditions</h3>
            <ul>
              <li>Timer reaches zero (Golden Hour expired)</li>
              <li>Patient health drops to 0%</li>
            </ul>
          </section>

          <div className="modal-footer">
            <button className="start-button" onClick={onClose}>
              Start Playing
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}


