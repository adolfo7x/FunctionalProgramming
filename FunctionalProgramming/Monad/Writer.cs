﻿using System;
using System.Collections.Generic;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public interface IWriter<TLog, TValue>
    {
        Tuple<TLog, TValue> ValueWithLog();
        TValue Value();
    }

    public static class Writer
    {
        public static IWriter<TLog, TValue> WithLog<TLog, TValue>(this TValue v, TLog log)
        {
            return new WriterImpl<TLog, TValue>(log, v);        
        }

        public static IWriter<TLog, TValue> WithLog<TLog, TValue>(this TValue v, Func<TValue, TLog> f)
        {
            return new WriterImpl<TLog, TValue>(f(v), v);
        }

        public static IWriter<IEnumerable<TLog>, TValue> WithLogs<TLog, TValue>(this TValue v, TLog log)
        {
            return new WriterImpl<IEnumerable<TLog>, TValue>(log.LiftEnumerable(), v);
        }

        public static IWriter<IEnumerable<TLog>, TValue> WithLogs<TLog, TValue>(this TValue v, Func<TValue, TLog> f)
        {
            return new WriterImpl<IEnumerable<TLog>, TValue>(f(v).LiftEnumerable(), v);
        }

        public static IWriter<TLog, Unit> LogOnly<TLog>(this TLog log)
        {
            return new WriterImpl<TLog, Unit>(log, Unit.Only);
        }

        public static IWriter<IEnumerable<TLog>, Unit> LogsOnly<TLog>(this TLog log)
        {
            return new WriterImpl<IEnumerable<TLog>, Unit>(log.LiftEnumerable(), Unit.Only);
        }

        public static IWriter<TLog, TValue> NoLog<TLog, TValue>(this TValue v, IMonoid<TLog> m)
        {
            return new WriterImpl<TLog, TValue>(m.MZero, v);
        }

        public static IWriter<TLog, TResult> Select<TLog, TInitial, TResult>(this IWriter<TLog, TInitial> m,
            Func<TInitial, TResult> f)
        {
            var logAndValue = m.ValueWithLog();
            return new WriterImpl<TLog, TResult>(logAndValue.Item1, f(logAndValue.Item2));
        }

        public static IWriter<IEnumerable<TLog>, TResult> SelectMany<TLog, TInitial, TResult>(
            this IWriter<IEnumerable<TLog>, TInitial> m, Func<TInitial, IWriter<IEnumerable<TLog>, TResult>> f)
        {
            var logAndValue = m.ValueWithLog();
            var newLogAndValue = f(logAndValue.Item2).ValueWithLog();
            return new WriterImpl<IEnumerable<TLog>, TResult>(EnumerableMonoid<TLog>.Only.MAppend(logAndValue.Item1, newLogAndValue.Item1), newLogAndValue.Item2);
        }

        public static IWriter<IEnumerable<TLog>, TSelect> SelectMany<TLog, TInitial, TResult, TSelect>(
            this IWriter<IEnumerable<TLog>, TInitial> m, Func<TInitial, IWriter<IEnumerable<TLog>, TResult>> f,
            Func<TInitial, TResult, TSelect> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => selector(a, b).NoLog(EnumerableMonoid<TLog>.Only)));
        }

        private class WriterImpl<TLog, TValue> : IWriter<TLog, TValue>
        {
            private readonly TLog _log;
            private readonly TValue _value;

            public WriterImpl(TLog log, TValue value)
            {
                _log = log;
                _value = value;
            }

            public Tuple<TLog, TValue> ValueWithLog()
            {
                return Tuple.Create(_log, _value);
            }

            public TValue Value()
            {
                return _value;
            }
        }        
    }
}
