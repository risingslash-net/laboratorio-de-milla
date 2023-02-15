import socket
import threading
import json
from datetime import datetime


class LobbyManager:
    def __init__(self):
        self.lobbies = {i: Lobby(i) for i in range(8)}

    def add_player(self, lobby_id, room_id, player_name, character_id, address):
        return self.lobbies[lobby_id].add_player(room_id, player_name, character_id, address)

    def remove_player(self, lobby_id, room_id, player_name):
        return self.lobbies[lobby_id].remove_player(room_id, player_name)

    def ready(self, lobby_id, room_id, player_name, ready):
        return self.lobbies[lobby_id][room_id][player_name].set_ready(ready)

    def add_spectator(self, lobby_id, room_id):
        return self.lobbies[lobby_id].add_spectator(room_id)

    def remove_spectator(self, lobby_id, room_id):
        return self.lobbies[lobby_id].remove_spectator(room_id)

    def get_room_status(self, lobby_id, room_id):
        return self.lobbies[lobby_id].get_room_status(room_id)

    def vote_for_map(self, lobby_id, room_id, player_name, map_name):
        return self.lobbies[lobby_id].vote_for_map(room_id, player_name, map_name)

    def start_game(self, lobby_id, room_id):
        return self.lobbies[lobby_id].start_game(room_id)

    def request_keepalives(self):
        for i in self.lobbies.values():
            i.request_keepalives()

    def keepalive_player(self, player_name):
        for i in self.lobbies.values():
            i.keepalive_player(player_name)

    def kick_player(self, player_name):
        for i in self.lobbies.values():
            for j in i.rooms.values():
                for k in j.players.values():
                    if k.player_name == player_name:
                        j.remove_player(k.player_name)
                        break


class Lobby:
    def __init__(self, lobby_id):
        self.rooms = {i: Room(i) for i in range(8)}
        self.lobby_id = lobby_id

    def add_player(self, room_id, player_name, character_id, address):
        return self.rooms[room_id].add_player(player_name, character_id, address)

    def remove_player(self, room_id, player_name):
        return self.rooms[room_id].remove_player(player_name)

    def add_spectator(self, room_id):
        return self.rooms[room_id].add_spectator()

    def remove_spectator(self, room_id):
        return self.rooms[room_id].remove_spectator()

    def get_room_status(self, room_id):
        return self.rooms[room_id].get_status()

    def vote_for_map(self, room_id, player_name, map_name):
        return self.rooms[room_id].vote_for_map(player_name, map_name)

    def start_game(self, room_id):
        return self.rooms[room_id].start_game()

    def request_keepalives(self):
        for i in self.rooms.values():
            i.request_keepalives()

    def keepalive_player(self, player_name):
        for i in self.rooms.values():
            i.keepalive_player(player_name)


class Room:
    def __init__(self, room_id):
        self.players = {}
        self.spectators = []
        self.map_votes = {}
        self.room_id = room_id

    def add_player(self, player_name, character_id, address):
        if len(self.players) >= 4:
            return False
        self.players[player_name] = Player(player_name, character_id, address)
        return True

    def remove_player(self, player_name):
        if player_name not in self.players:
            return False
        del self.players[player_name]
        return True

    def add_spectator(self):
        if len(self.spectators) >= 12:
            return False
        self.spectators.append(None)
        return True

    def remove_spectator(self):
        if len(self.spectators) == 0:
            return False
        self.spectators.pop()
        return True

    def get_status(self):
        return {
            'players': self.players,
            'spectators': len(self.spectators),
            'map_votes': self.map_votes
        }

    def vote_for_map(self, player_name, map_name):
        if player_name not in self.players:
            return False
        self.players[player_name]['vote'] = map_name
        if map_name in self.map_votes:
            self.map_votes[map_name] += 1
        else:
            self.map_votes[map_name] = 1
        return True

    def start_game(self):
        if len(self.players) < 2:
            return False
        map_name = max(self.map_votes, key=self.map_votes.get)
        # start game with map_name
        return True

    def request_keepalives(self):
        for i in self.players.values():
            i.request_keepalive()

    def keepalive_player(self, player_name):
        for i in self.players.values():
            if (i.player_name == player_name):
                i.keepalive()


class Player:
    def __init__(self, player_name, character_id, address):
        self.player_name = player_name
        self.character_id = character_id
        self.ready = False
        self.vote = None
        self.address = address
        self.missed_keepalive_count = 0
        self.mod_list = []

    def set_ready(self, ready):
        if len(self.players) >= 4:
            return False
        self.ready = ready
        return self.ready

    def request_keepalive(self):
        if self.missed_keepalive_count > 2:
            print(f'Kicking {self.player_name} from the server for idling.')
            response = {'status': 'success'}
            response['message'] = 'kicked'
            response['data'] = 'You were kicked from the server for failing to respond to the keepalive pings.'
            server.socket.sendto(json.dumps(response).encode(), self.address)
            server.lobby_manager.kick_player(self.player_name)
        else:
            response = {'status': 'success'}
            response['message'] = 'request_keepalive'
            response['data'] = f'Keepalives Missed: {self.missed_keepalive_count}'
            server.socket.sendto(json.dumps(response).encode(), self.address)
        
        self.missed_keepalive_count += 1

    def keepalive(self):
        self.missed_keepalive_count = 0

    def toJSON(self):
        return json.dumps(self, default=lambda o: o.__dict__,
                          sort_keys=True, indent=4)

class PlayerEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, Player):
            return {
                "player_name": obj.player_name,
                "character_id": obj.character_id,
                "ready": obj.ready,
                "vote": obj.vote,
                "address": obj.address,
                "missed_keepalive_count": obj.missed_keepalive_count,
                "mod_list": obj.mod_list
            }
        return super().default(obj)

class LobbyServer:
    def __init__(self, address=('localhost', 20232)):
        self.versionNumber = "1.0.230215"
        print(f'[Phantom Chase v{self.versionNumber}]\n')
        print(f'Creating a new instance of the Phantom lobby server for {address}\n')
        print('Use Ctrl+Break (Pause) to quit.\n')
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.socket.bind(address)
        self.lobby_manager = LobbyManager()
        self.dt_last_keepalive = datetime.now()

    def start(self):
        while True:
            data, address = self.socket.recvfrom(1024)
            threading.Thread(target=self.handle_client, args=(data, address)).start()
            time_since_keepalive = datetime.now() - self.dt_last_keepalive
            seconds_since_keepalive = time_since_keepalive.seconds
            while seconds_since_keepalive >= 60:
                self.lobby_manager.request_keepalives()
                self.dt_last_keepalive = datetime.now()
                seconds_since_keepalive -= 60

    def handle_client(self, data, address):
        message = json.loads(data.decode())
        command = message['command']
        args = message['args']
        response = {'status': 'error'}

        print(f'addr({address}) sent: {command} with args {args}')

        if command == 'add_player':
            lobby_id, room_id, player_name, character_id = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            character_id = int(character_id)
            success = self.lobby_manager.add_player(lobby_id, room_id, player_name, character_id, address)
            if success:
                response['status'] = 'success'
        elif command == 'remove_player':
            lobby_id, room_id, player_name = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            success = self.lobby_manager.remove_player(lobby_id, room_id, player_name)
            if success:
                response['status'] = 'success'
        elif command == 'keepalive_player':
            player_name = args
            success = self.lobby_manager.keepalive_player(player_name)
            if success:
                response['status'] = 'success'
        elif command == 'add_spectator':
            lobby_id, room_id = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            success = self.lobby_manager.add_spectator(lobby_id, room_id)
            if success:
                response['status'] = 'success'
        elif command == 'remove_spectator':
            lobby_id, room_id = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            success = self.lobby_manager.remove_spectator(lobby_id, room_id)
            if success:
                response['status'] = 'success'
        elif command == 'get_room_status':
            lobby_id, room_id = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            response['status'] = 'success'
            response['data'] = self.lobby_manager.get_room_status(lobby_id, room_id)
        elif command == 'vote_for_map':
            lobby_id, room_id, player_name, map_name = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            success = self.lobby_manager.vote_for_map(lobby_id, room_id, player_name, map_name)
            if success:
                response['status'] = 'success'
        elif command == 'ready':
            lobby_id, room_id, player_name, ready = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            ready = bool(ready)
            success = self.lobby_manager.ready(lobby_id, room_id, player_name, ready)
            if success:
                response['status'] = 'success'
        elif command == 'start_game':
            lobby_id, room_id = args
            lobby_id = int(lobby_id)
            room_id = int(room_id)
            success = self.lobby_manager.start_game(lobby_id, room_id)
            if success:
                response['status'] = 'success'
        else:
            response['message'] = 'Invalid command'

        self.socket.sendto(json.dumps(response, cls=PlayerEncoder).encode(), address)


if __name__ == '__main__':
    server = LobbyServer()
    server.start()
