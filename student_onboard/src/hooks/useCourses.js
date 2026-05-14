// src/hooks/useCourses.js
import { useState, useEffect, useCallback } from "react";
import { coursesApi } from "../services/api";

export function useCourses() {
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState(null);

  const fetch = useCallback(async () => {
    setLoading(true); setError(null);
    try {
      const res = await coursesApi.getAll();
      setCourses(res.data);
    } catch (err) {
      setError(err.message);
      setCourses([]); // don't leave stale/undefined state
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetch(); }, [fetch]);

  const createCourse = useCallback(async (payload) => {
    const res = await coursesApi.create(payload);
    setCourses((prev) => [...prev, res.data]);
    return res.data;
  }, []);

  const updateCourse = useCallback(async (id, payload) => {
    const res = await coursesApi.update(id, payload);
    setCourses((prev) => prev.map((c) => (c.id === id ? res.data : c)));
    return res.data;
  }, []);

  const deleteCourse = useCallback(async (id) => {
    await coursesApi.delete(id);
    setCourses((prev) => prev.filter((c) => c.id !== id));
  }, []);

  return { courses, loading, error, refetch: fetch, createCourse, updateCourse, deleteCourse };
}
