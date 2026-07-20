import React, { useState } from 'react';
import './ModuleProgress.css';

export const ModuleProgress = ({
  modules = [],
  currentModule = '',
  onModuleClick = null,
  viewType = 'grid'
}) => {
  const [selectedModule, setSelectedModule] = useState(null);

  const handleModuleClick = (module) => {
    setSelectedModule(module);
    onModuleClick?.(module);
  };

  if (viewType === 'timeline') {
    return (
      <div className="module-progress-timeline">
        {modules.map((module, idx) => (
          <div key={idx} className="timeline-item">
            <div className={`timeline-dot ${module.isCompleted ? 'completed' : ''}`}>
              {module.isCompleted ? '✓' : idx + 1}
            </div>
            <div className="timeline-content">
              <h4 className="timeline-title">{module.moduleName}</h4>
              {module.completedDate && (
                <p className="timeline-date">
                  Completed: {new Date(module.completedDate).toLocaleDateString()}
                </p>
              )}
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (viewType === 'list') {
    return (
      <div className="module-progress-list">
        {modules.map((module, idx) => (
          <div
            key={idx}
            className={`module-item ${module.isCompleted ? 'completed' : ''} ${
              module.moduleName === currentModule ? 'current' : ''
            }`}
            onClick={() => handleModuleClick(module)}
            role="button"
            tabIndex={0}
          >
            <span className="module-checkbox">
              {module.isCompleted ? '☑️' : '☐'}
            </span>
            <span className="module-name">{module.moduleName}</span>
            {module.completedDate && (
              <span className="module-date">
                {new Date(module.completedDate).toLocaleDateString()}
              </span>
            )}
          </div>
        ))}
      </div>
    );
  }

  // Default: grid view
  return (
    <div className="module-progress-grid">
      {modules.map((module, idx) => (
        <div
          key={idx}
          className={`module-card ${module.isCompleted ? 'completed' : ''} ${
            module.moduleName === currentModule ? 'current' : ''
          }`}
          onClick={() => handleModuleClick(module)}
          role="button"
          tabIndex={0}
        >
          <div className="module-indicator">
            {module.isCompleted ? '✓' : module.moduleNumber}
          </div>
          <div className="module-name">{module.moduleName}</div>
          {module.completedDate && (
            <div className="module-date">
              {new Date(module.completedDate).toLocaleDateString()}
            </div>
          )}
          {module.moduleName === currentModule && (
            <div className="module-badge">Current</div>
          )}
        </div>
      ))}
    </div>
  );
};
