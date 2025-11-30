using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Interfaces
{
    public interface IBackend
    {
        /**
         * Takes a node type, a LayerNum and a planePos to the backend to create a node.
         *
         * LayerNum 0 -> Time Slice T
         * LayerNum 1 -> Time Slice T-1
         * LayerNum 2 -> Time Slice T-2
         *
         * When null is returned, no node was created on the backend.
         * The planePos is used directly in the backend.
         *
         * The returned GUID is a reference to the backend node object. Can be used to interact with the object (delete, link).
         */
        GUID? PlaceNode(NodeDTO nodeType, int LayerNum, Vector2 planePos);
        
        /**
         * Takes a node backend id to delete a node.
         * Returns true if it could be deleted. Else false
         */
        bool DeleteNode(GUID nodeBackendId);
        
        /**
         * Takes two node uids to the backend.
         * Returns true and a connection uid if the nodes could be connected.
         * Otherwise, returns false and null.
         */
        bool LinkNodes(GUID backendIdA, GUID backendIdB, out GUID? connectionID);
        
        /**
         * Takes a connection id (NOT A NODE ID) to remove the connection and unlink the nodes that are connected.
         * Returns true if a connection with this id could be unlinked.
         */
        bool UnlinkNodes(GUID connectionId);
        
        /**
         * Returns the progress between 0-1 on the current edge and the current edge of the packet.
         * If the energy packet does not exist returns -1 and null;
         *
         * This function is called per FixedUpdate on a Frontend Energy Packet Game Object. Between frames interpolation might be needed
         */
        float GetEnergyPacketProgress(GUID packetID, out AbstractNodeInstance sourceNode, out AbstractNodeInstance targetNode);//todo had to change it to know the source adn target
        float GetPacketFrame(GUID packetID);//TODO either seperate function or change the above to return frame as well

        /**
         * Called on FixedUpdate by the frontend to trigger a simulation step on the backend.
         */
        void tick(long tickCount);

        // TimeLayer GetTimeLayer(int layerNum); //change time layer to whatever applies
    }

    public enum NodeDTO //TODO NodeType got renamed to NodeDTO to prevent double naming with Backend.NodeType
    {
        GENERATOR,
        RIPPLE
    }
}