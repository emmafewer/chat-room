# Instructions

## Server
1. Start the server in a Docker container by running the following command:
```sh ./run_worker_start_local.sh ```
2. Note that it is running on Port 13000

## Client
1. Use the tool netcat to spin up a client
   ```nc localhost 13000```
* If this doesn't work you can also try
   ```telnet localhost 13000```
2. Create as many clients as you'd like by repeating step 1. You should be able to chat with each other.

  <img width="483" alt="Screenshot 2024-11-26 at 4 52 58 PM" src="https://github.com/user-attachments/assets/0644b7d7-eac2-446c-bddd-e6c659939617">
