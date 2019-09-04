using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using Tmds.DBus;

namespace VectorBLE
{
    public class VectorBLE
    {
        const string ADAPTER_NAME = "hci0";

        public async Task<object> ScanForRobots(string filter = null)
        {
            string adapterObjectPath = $"/org/bluez/{ADAPTER_NAME}";
            IAdapter1 adapter = Connection.System.CreateProxy<IAdapter1>(BluezConstants.DbusService, adapterObjectPath);
            if (adapter == null)
            {
                Console.WriteLine($"Bluetooth adapter '{ADAPTER_NAME}' not found.");
            }

            IReadOnlyList<IDevice1> devices = await adapter.GetDevicesAsync();
            Console.WriteLine($"{devices.Count} devices found");
            foreach (IDevice1 device in devices)
            {
                Console.WriteLine(device);
                string name = await device.GetNameAsync();
                Console.WriteLine(name);
            }

            return devices;
        }
    }

    // Extensions that make it easier to get a D-Bus object or read a characteristic value.
    // "borrowed" from https://stackoverflow.com/questions/53933345/utilizing-bluetooth-le-on-raspberry-pi-using-net-core
    static class Extensions
    {
        public static Task<IReadOnlyList<IDevice1>> GetDevicesAsync(this IAdapter1 adapter)
        {
            return GetProxiesAsync<IDevice1>(adapter, BluezConstants.DeviceInterface);
        }

        public static async Task<IDevice1> GetDeviceAsync(this IAdapter1 adapter, string deviceAddress)
        {
            var devices = await GetProxiesAsync<IDevice1>(adapter, BluezConstants.DeviceInterface);

            var matches = new List<IDevice1>();
            foreach (var device in devices)
            {
                if (String.Equals(await device.GetAddressAsync(), deviceAddress, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(device);
                }
            }

            // BlueZ can get in a weird state, probably due to random public BLE addresses.
            if (matches.Count > 1)
            {
                throw new Exception($"{matches.Count} devices found with the address {deviceAddress}!");
            }

            return matches.FirstOrDefault();
        }

        public static async Task<IGattService1> GetServiceAsync(this IDevice1 device, string serviceUUID)
        {
            var services = await GetProxiesAsync<IGattService1>(device, BluezConstants.GattServiceInterface);

            foreach (var service in services)
            {
                if (String.Equals(await service.GetUUIDAsync(), serviceUUID, StringComparison.OrdinalIgnoreCase))
                {
                    return service;
                }
            }

            return null;
        }

        public static async Task<IGattCharacteristic1> GetCharacteristicAsync(this IGattService1 service, string characteristicUUID)
        {
            var characteristics = await GetProxiesAsync<IGattCharacteristic1>(service, BluezConstants.GattCharacteristicInterface);

            foreach (var characteristic in characteristics)
            {
                if (String.Equals(await characteristic.GetUUIDAsync(), characteristicUUID, StringComparison.OrdinalIgnoreCase))
                {
                    return characteristic;
                }
            }

            return null;
        }

        public static async Task<byte[]> ReadValueAsync(this IGattCharacteristic1 characteristic, TimeSpan timeout)
        {
            var options = new Dictionary<string, object>();
            var readTask = characteristic.ReadValueAsync(options);
            var timeoutTask = Task.Delay(timeout);

            await Task.WhenAny(new Task[] { readTask, timeoutTask });
            if (!readTask.IsCompleted)
            {
                throw new TimeoutException("Timed out waiting to read characteristic value.");
            }

            return await readTask;
        }

        private static async Task<IReadOnlyList<T>> GetProxiesAsync<T>(IDBusObject rootObject, string interfaceName)
        {
            // Console.WriteLine("GetProxiesAsync called.");
            var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
            var objects = await objectManager.GetManagedObjectsAsync();

            var matchingObjects = objects
              .Where(obj => obj.Value.Keys.Contains(interfaceName))
              .Select(obj => obj.Key)
              .Where(objectPath => objectPath.ToString().StartsWith($"{rootObject.ObjectPath}/"));

            var proxies = matchingObjects
              .Select(objectPath => Connection.System.CreateProxy<T>(BluezConstants.DbusService, objectPath))
              .ToList();

            // Console.WriteLine($"GetProxiesAsync returning {proxies.Count} proxies of type {typeof(T)}.");
            return proxies;
        }
    }
}
