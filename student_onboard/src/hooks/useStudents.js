// src/hooks/useStudents.js
import { useState, useEffect, useCallback, useRef } from "react";
import { studentsApi } from "../services/api";

export function useStudents(filters = {}) {
  const [students, setStudents] = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [error,    setError]    = useState(null);
  const [meta,     setMeta]     = useState({ total: 0, page: 1, pages: 1 });

  const filtersRef = useRef(filters);
  filtersRef.current = filters;
  const filtersKey = JSON.stringify(filters);

  const initialLoadDone = useRef(false);

  const fetch = useCallback(async () => {
    if (!initialLoadDone.current) { setLoading(true); setError(null); }
    try {
      const res = await studentsApi.getAll(filtersRef.current);
      setStudents(res.data);
      setMeta({ total: res.total, page: res.page, pages: res.pages });
    } catch (err) {
      if (!initialLoadDone.current) {
        setError(err.message);
        setStudents([]); // prevent crash
      }
    } finally {
      setLoading(false);
      initialLoadDone.current = true;
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filtersKey]);

  useEffect(() => {
    fetch();
    const interval = setInterval(fetch, 30000); // Auto-refresh every 30s
    return () => clearInterval(interval);
  }, [fetch]);

  const updateStatus = useCallback(async (id, status) => {
    const res = await studentsApi.updateStatus(id, status);
    setStudents((prev) => prev.map((s) => (s.id === id ? res.data : s)));
    return res.data;
  }, []);

  return { students, loading, error, meta, refetch: fetch, updateStatus };
}

export function useStudentStats() {
  const [stats,   setStats]   = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchStats = useCallback(async () => {
    setLoading(true);
    try {
      const res = await studentsApi.stats();
      setStats(res.data);
    } catch {
      setStats({ total: 0, approved: 0, pending: 0, blocked: 0 });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchStats();
    const interval = setInterval(fetchStats, 30000); // Auto-refresh every 30s
    return () => clearInterval(interval);
  }, [fetchStats]);

  return { stats, loading, refetchStats: fetchStats };
}
