export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAtUtc: string;
  sourceEventId: string;
}
