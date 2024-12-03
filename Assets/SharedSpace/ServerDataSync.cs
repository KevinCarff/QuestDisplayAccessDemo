using UnityEngine;
using Mirror;

public class ServerDataSync : NetworkBehaviour
{
    [SyncVar] private Vector3 serverVector; // The server-stored position
    [SyncVar] private Vector3 serverRotation; // The server-stored rotation

    // Command to set the server's position and rotation, requiresAuthority = false
    [Command(requiresAuthority = false)]
    public void CmdSetServerValues(Vector3 position, Vector3 rotation)
    {
        serverVector = position;
        serverRotation = rotation;

        Debug.Log($"Server received new values: Position = {position}, Rotation = {rotation}");
    }

    // Command to retrieve the server's position and rotation
    [Command(requiresAuthority = false)]
    public void CmdGetServerValues(NetworkConnectionToClient sender = null)
    {
        Debug.Log("Server is sending stored values back to the client.");

        // Call the client RPC to send the values back
        TargetReceiveServerValues(sender, serverVector, serverRotation);
    }

    // TargetRPC to send values back to the client that requested them
    [TargetRpc]
    private void TargetReceiveServerValues(NetworkConnection target, Vector3 position, Vector3 rotation)
    {
        Debug.Log($"Client received server values: Position = {position}, Rotation = {rotation}");

        // You can implement additional logic here to handle the received values on the client
    }
}
