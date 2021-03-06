﻿using System;
using System.Collections.Generic;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.Collector.Pipeline;

namespace InfluxDB.Collector
{
    public abstract class MetricsCollector : IPointEmitter, IDisposable
    {
        public void Increment(string measurement, long count = 1, IReadOnlyDictionary<string, string> tags = null)
        {
            Write(measurement, new Dictionary<string, object> { { "count", count } }, tags);
        }

        public void Measure(string measurement, object value, IReadOnlyDictionary<string, string> tags = null)
        {
            Write(measurement, new Dictionary<string, object> { { "value", value } }, tags);
        }

        public IDisposable Time(string measurement, IReadOnlyDictionary<string, string> tags = null)
        {
            return new StopwatchTimer(this, measurement, tags);
        }

        public CollectorConfiguration Specialize()
        {
            return new CollectorConfiguration(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }

        public void Write(string measurement, IReadOnlyDictionary<string, object> fields, IReadOnlyDictionary<string, string> tags = null, DateTime? timestamp = null)
        {
            try
            {
                var point = new PointData(measurement, fields, tags, timestamp ?? DateTime.UtcNow);
                Emit(new[] { point });
            }
            catch (Exception ex)
            {
                CollectorLog.WriteLine("Failed to write point: {0}", ex);
            }
        }

        void IPointEmitter.Emit(PointData[] points)
        {
            Emit(points);
        }

        protected abstract void Emit(PointData[] points);
    }
}
