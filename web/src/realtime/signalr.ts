import * as signalR from '@microsoft/signalr';
import { getToken } from '../api/client';

export function createChatConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl('/hubs/chat', { accessTokenFactory: () => getToken() ?? '' })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}
