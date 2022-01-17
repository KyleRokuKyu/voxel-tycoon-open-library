﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelTycoon.Buildings;

namespace VTOL.StorageNetwork
{
	/// <summary>
	/// This class is used to collect all the Siblings found by <see cref="VoxelTycoon.Buildings.StorageBuildingManager.FindSiblings(StorageNetworkBuilding)"/>
	/// and allows them to be removed based on specified conditions.
	/// </summary>
	public class PotentialConnectionArgs
	{
		private readonly IList<StorageBuildingSibling> _siblings;
		private readonly IList<StorageBuildingSibling> _addedConnections = new List<StorageBuildingSibling>();
		private readonly IList<PotentialConnection> _connections;
		private ISet<int> _buildingIds;

		internal PotentialConnectionArgs(IList<StorageBuildingSibling> siblings)
		{
			_siblings = siblings;
			_connections = new List<PotentialConnection>(_siblings.Count);

			foreach (StorageBuildingSibling sibling in _siblings)
			{
				_connections.Add(new PotentialConnection(sibling.Building));
			}
		}

		public IEnumerable<PotentialConnection> Connections => _connections;
		internal IList<StorageBuildingSibling> AddedConnections => _addedConnections;

		/// <summary>
		/// Create a connection with a building not detected by <see cref="StorageBuildingManager.FindSiblings(StorageNetworkBuilding)"/>.
		/// </summary>
		/// <param name="storageBuildingSibling">The <see cref="StorageBuildingSibling"/> containing the information to create a connection.</param>
		/// <remarks>A <see cref="StorageBuildingSibling"/> can be created with <see cref="StorageNetworkUtils.CreateSiblingOf(StorageNetworkBuilding, StorageNetworkBuilding, bool)"/>.</remarks>
		public void AddConnection(StorageBuildingSibling storageBuildingSibling)
		{
			RegisterBuildingIds();

			int id = storageBuildingSibling.Building.Id;

			if (_buildingIds.Contains(id))
			{
				throw new InvalidOperationException($"Building with ID: {id} was already detected or has already been added.");
			}

			_addedConnections.Add(storageBuildingSibling);
			_buildingIds.Add(id);
		}
		
		/// <summary>
		/// Removes all the StorageBuildingSiblings that have been canceled.
		/// </summary>
		/// <returns>Returns a list with the StorageBuildingSiblings which are not canceled.</returns>
		internal IList<StorageBuildingSibling> RemoveCanceled()
		{
			Trace.Assert(_connections.Count == _siblings.Count);

			for (int i = _connections.Count - 1; i >= 0; i--)
			{
				if (_connections[i].IsCanceled)
				{
					Trace.Assert(_connections[i].Building.Id == _siblings[i].Building.Id);

					_siblings.RemoveAt(i);
				}
			}

			return _siblings;
		}

		//When a new connection is added, the connected building cannot be already detected by the normal Voxel Tycoon detection. After all, this would allow to force a connection with an already detected building, bypassing other registered filters.
		//To check if a building was already detected a list with id's is created from the list of detected buildings (_siblings). This only has to happen when a custom connection is added with AddConnection(), and only once.
		//When an instance of _buildingId's already exists (!= null), there is no need to create a new list, since the detected buildings wont change during the lifecycle of this instance of PotentialConnectionArgs.
		private void RegisterBuildingIds()
		{
			if (_buildingIds == null)
			{
				_buildingIds = new HashSet<int>();

				foreach (StorageBuildingSibling sibling in _siblings)
				{
					_buildingIds.Add(sibling.Building.Id);
				}
			}
		}
	}
}