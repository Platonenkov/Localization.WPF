using System;
using System.Collections.Generic;

namespace Localization.WPF.Extensions
{
    /// <summary>Класс методов расширений сервисных функций</summary>
    internal static class ServiceExtensions
    {
        /// <summary>Выполнить действие для каждого элемента перечня</summary>
        /// <typeparam name="T">Тип элементов перечня</typeparam>
        /// <param name="collection">Перечень элементов</param>
        /// <param name="action">Действие, выполняемое для всех элементов перечня</param>
        public static void Foreach<T>(this IEnumerable<T> collection, Action<T> action) { foreach(var v in collection) action(v); }
    }
}