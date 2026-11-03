using System;
using System.Collections.Generic;
using System.Linq;

namespace NotificationLibrary
{
    public enum NotificationType
    {
        Info, 
        Warning,
        Error,
        System
    }

    public enum SortNotificationBy
    {
        Date,
        Priority,
        Title
    }
    public class Notificationclass 
    {
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
    public NotificationType Type { get; set; } // Info, Warning, Error, System
    public DateTime CreatedAt { get; set; }
    public int Priority { get; set; } // 0 (низкий) до 5 (высокий)
    public bool IsRead { get; set; }
    }

    public class NotificationFilterOptions
    {
        public bool? IsRead { get; set; }
        public NotificationType[]? Types { get; set; } // список нужных типов
        public string? SearchText { get; set; }
        public int? MinPriority { get; set; }
        public SortNotificationBy? SortBy { get; set; } // по Date, Priority, Title
        public bool Descending { get; set; }
    }

    public static class NotificationService
    {
        public static IEnumerable<Notificationclass> FilterAndSort(
            IEnumerable<Notificationclass>? notifications,
            NotificationFilterOptions? options)
        {
            if (notifications == null)
            {
                return Enumerable.Empty<Notificationclass>();
            }

            var query = notifications.AsQueryable();

            if (options == null)
            {
                return query.ToList();
            }

            // 1. Фильтрация по статусу прочтения
            if (options.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == options.IsRead.Value);
            }

            // 2. Фильтрация по типам уведомлений
            if (options.Types != null && options.Types.Length > 0)
            {
                query = query.Where(n => options.Types.Contains(n.Type));
            }

            // 3. Фильтрация по минимальному приоритету
            if (options.MinPriority.HasValue)
            {
                query = query.Where(n => n.Priority >= options.MinPriority.Value);
            }

            // 4. Поиск по тексту (заголовок или содержимое)
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                var text = options.SearchText.Trim();
                query = query.Where(n =>
                    (n.Title != null && n.Title.Contains(text, StringComparison.OrdinalIgnoreCase)) || (n.Content != null && n.Content.Contains(text, StringComparison.OrdinalIgnoreCase)));
            }

            // 5. Сортировка
            if (options.SortBy.HasValue)
            {
                query = options.SortBy.Value switch
                {
                    SortNotificationBy.Date => options.Descending
                        ? query.OrderByDescending(n => n.CreatedAt)
                        : query.OrderBy(n => n.CreatedAt),

                    SortNotificationBy.Priority => options.Descending
                        ? query.OrderByDescending(n => n.Priority)
                        : query.OrderBy(n => n.Priority),

                    SortNotificationBy.Title => options.Descending
                        ? query.OrderByDescending(n => n.Title)
                        : query.OrderBy(n => n.Title),

                    _ => query
                };
            }

            return query.ToList();
        }
    }
}
