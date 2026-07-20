import React, { useState, useEffect } from 'react';
import './ProgressBar.css';

export const ProgressBar = ({
  percentage = 0,
  label = '',
  height = '8px',
  animated = true,
  showMilestones = false,
  milestones = [],
  color = 'linear-gradient(90deg, #4CAF50, #45a049)',
  backgroundColor = '#f0f0f0'
}) => {
  const [displayPercentage, setDisplayPercentage] = useState(0);

  useEffect(() => {
    if (!animated) {
      setDisplayPercentage(percentage);
      return;
    }

    // Animate from 0 to target percentage
    let current = 0;
    const increment = percentage / 20;
    const interval = setInterval(() => {
      current += increment;
      if (current >= percentage) {
        setDisplayPercentage(percentage);
        clearInterval(interval);
      } else {
        setDisplayPercentage(Math.round(current * 100) / 100);
      }
    }, 30);

    return () => clearInterval(interval);
  }, [percentage, animated]);

  return (
    <div className="progress-container">
      {label && <div className="progress-label">{label}</div>}

      <div
        className="progress-bar-wrapper"
        style={{
          height: height,
          backgroundColor: backgroundColor,
          borderRadius: '4px',
          overflow: 'hidden',
          position: 'relative'
        }}
      >
        <div
          className="progress-bar-fill"
          style={{
            width: `${displayPercentage}%`,
            background: color,
            height: '100%',
            borderRadius: '4px',
            transition: animated ? 'width 0.3s ease' : 'none'
          }}
        />

        {showMilestones && milestones.length > 0 && (
          <div className="progress-milestones">
            {milestones.map((milestone, idx) => (
              <div
                key={idx}
                className="milestone"
                style={{
                  position: 'absolute',
                  left: `${milestone.percentage}%`,
                  top: '0',
                  height: '100%',
                  width: '2px',
                  backgroundColor: 'rgba(0, 0, 0, 0.2)',
                  title: milestone.label
                }}
              />
            ))}
          </div>
        )}
      </div>

      <div className="progress-percentage">
        {displayPercentage.toFixed(1)}% Complete
      </div>
    </div>
  );
};
