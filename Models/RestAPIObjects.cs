using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Management.Automation;

namespace WaykDen.Models
{
    public class SessionInProgressObject
    {
        public string ID {get; set;}
        public string ClientDenID {get; set;}
        public string ServerDenID {get; set;}
        public string ClientConnectionID {get; set;}
        public string ClientMachineName {get; set;}
        public string ClientUserAgent {get; set;}
        public string ClientUserID {get; set;}
        public string ClientUsername {get; set;}
        public string ServerConnectionID {get; set;}
        public string ServerMachineName {get; set;}
        public string ServerUserAgent {get; set;}
        public string ServerUserID {get; set;}
        public string ServerUsername {get; set;}
        public DateTime StartTime {get; set;}
        public DateTime LastUpdate {get; set;}
    }
    public class SessionTerminatedObject
    {
        public string ID {get; set;}
        public string ClientDenID {get; set;}
        public string ServerDenID {get; set;}
        public string ClientConnectionID {get; set;}
        public string ClientMachineName {get; set;}
        public string ClientUserAgent {get; set;}
        public string ClientUserID {get; set;}
        public string ClientUsername {get; set;}
        public string ServerConnectionID {get; set;}
        public string ServerMachineName {get; set;}
        public string ServerUserAgent {get; set;}
        public string ServerUserID {get; set;}
        public string ServerUsername {get; set;}
        public DateTime StartTime {get; set;}
        public DateTime EndTime {get; set;}
        public bool EndedGracefully {get; set;}
    }
    public class Session
    {
        public Oid _id {get; set;} = new Oid();
        public string session_id {get; set;} = string.Empty;
        public string client_den_id {get; set;} = string.Empty;
        public string server_den_id {get; set;} = string.Empty;
        public PeerInfo client_info {get; set;} = new PeerInfo();
        public PeerInfo server_info {get; set;} = new PeerInfo();
        public TimeStamp start_timestamp {get; set;} = new TimeStamp();
        public TimeStamp end_timestamp {get; set;} = new TimeStamp();
        public TimeStamp last_update {get; set;} = new TimeStamp();
        public bool ended_gracefully {get; set;} = false;

        public object ToSessionObject()
        {
            DateTime started = RestAPIUtils.GetDateTime(this.start_timestamp);
            DateTime ended = RestAPIUtils.GetDateTime(this.end_timestamp);
            if(DateTime.Compare(started, ended) > 0)
            {
                return new SessionInProgressObject
                {
                    ID = this.session_id,
                    ClientDenID = this.client_den_id,
                    ServerDenID = this.server_den_id,
                    ClientConnectionID = this.client_info.connection_id,
                    ClientMachineName = this.client_info.machine_name,
                    ClientUserAgent = this.client_info.user_agent,
                    ClientUserID = this.client_info.user_id,
                    ClientUsername = this.client_info.username,
                    ServerConnectionID = this.server_info.connection_id,
                    ServerMachineName = this.server_info.machine_name,
                    ServerUserAgent = this.server_info.user_agent,
                    ServerUserID = this.server_info.user_id,
                    ServerUsername = this.server_info.username,
                    StartTime = RestAPIUtils.GetDateTime(this.start_timestamp),
                    LastUpdate = RestAPIUtils.GetDateTime(this.last_update),
                };
            }

            return new SessionTerminatedObject()
            {
                ID = this.session_id,
                ClientDenID = this.client_den_id,
                ServerDenID = this.server_den_id,
                ClientConnectionID = this.client_info.connection_id,
                ClientMachineName = this.client_info.machine_name,
                ClientUserAgent = this.client_info.user_agent,
                ClientUserID = this.client_info.user_id,
                ClientUsername = this.client_info.username,
                ServerConnectionID = this.server_info.connection_id,
                ServerMachineName = this.server_info.machine_name,
                ServerUserAgent = this.server_info.user_agent,
                ServerUserID = this.server_info.user_id,
                ServerUsername = this.server_info.username,
                StartTime = RestAPIUtils.GetDateTime(this.start_timestamp),
                EndTime = RestAPIUtils.GetDateTime(this.end_timestamp),
                EndedGracefully = this.ended_gracefully
            };
        }
    }

    public class TimeStamp
    {
        [JsonProperty("$date")]
        public Date date {get; set;}
    }

    public class Date
    {
        [JsonProperty("$numberlong")]
        public long numberLong {get; set;} = 0;
    }

    public class PeerInfo
    {
        public string connection_id {get; set;} = string.Empty;
        public string machine_name {get; set;} = string.Empty;
        public string user_agent {get; set;} = string.Empty;
        public string user_id {get; set;} = string.Empty;
        public string username {get; set;} = string.Empty;
    }

    public class Oid
    {
        [JsonProperty("$oid")]
        public string oid {get; set;}
    }

    public class UserObject
    {
        public string ID {get; set;}
        public string Username {get; set;}
        public string Name {get; set;}
        public string Email {get; set;}
        public string LicenseID {get; set;}
    }

    public class User
    {

        public Oid _id {get; set;} = new Oid();
        public string account_id {get; set;} = string.Empty;
        public string username {get; set;} = string.Empty;
        public string given_name {get; set;} = string.Empty;
        public string family_name {get; set;} = string.Empty;
        public string name {get; set;} = string.Empty;
        public string email {get; set;} = string.Empty;
        public Address address {get; set;} = new Address();
        public string phone_number {get; set;} = string.Empty;
        public string picture {get; set;} = string.Empty;
        public string locale {get; set;} = string.Empty;
        public Oid license_id {get; set;} = new Oid();

        public UserObject ToUserObject()
        {
            return new UserObject
            {
                ID = this._id.oid,
                Username = this.username,
                Name = this.name,
                Email = this.email,
                LicenseID = license_id.oid
            };
        }
    }

    public class Address
    {
        public string street_address {get; set;}
        public string locality {get; set;}
        public string region {get; set;}
        public string postal_code {get; set;}
        public string country {get; set;}
    }

    public class LicenseObject
    {
        public string ID {get; set;}
        public string SerialNumber {get; set;}
        public DateTime Expiration {get; set;}
        public string Product {get; set;}
        public bool Trial {get; set;}
        public int Count {get; set;}
        public string Type {get; set;}
    }

    public class License
    {
        public Oid _id {get; set;} = new Oid();
        public string serial_number {get; set;} = string.Empty;
        public LicenseInfo info {get; set;} = new LicenseInfo();

        public LicenseObject ToLicenseObject()
        {
            return new LicenseObject
            {
                ID = this._id.oid,
                SerialNumber = this.serial_number,
                Expiration = RestAPIUtils.GetDateTime(this.info.expiration),
                Product = this.info.product.ToString(),
                Trial = this.info.is_trial,
                Count = this.info.count,
                Type = this.info.type_.ToString()
            };
        }
    }

    public class LicenseInfo
    {
        public TimeStamp expiration {get; set;} = new TimeStamp();
        public TimeStamp generation {get; set;} = new TimeStamp();
        public LicoriceProduct product {get; set;} = LicoriceProduct.Unknown;
        public bool is_trial {get; set;} = false;
        public int count {get; set;} = 0;
        public LicoriceType type_ {get; set;} = LicoriceType.None;
    }

    public enum LicoriceProduct
    {
        Unknown = 0,
        RemoteDesktopManager = 1,
        PasswordVaultManager = 2,
        DevolutionsServer = 3,
        WaykNow = 20,
        WaykDen = 21
    }

    public enum LicoriceType
    {
        None = 0,
        Free = 1,
        Basic = 2,
        Professional = 3,
        Enterprise = 4,
        SmallBusiness = 5,
        Corporate = 6,
        Platinum = 7,
        Site = 10,
        Country = 11,
        Global = 12,
        Cal = 20
    }

    public class RestAPIUtils
    {
        public static DateTime GetDateTime(TimeStamp time)
        {
            if(time == null || time.date == null)
            {
                return new DateTime();
            }
            DateTime denServerBase = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long tick = denServerBase.Ticks + (time.date.numberLong * (long)Math.Pow(10, 4));
            DateTime t = new DateTime(tick, DateTimeKind.Utc);
            return t.ToLocalTime();
        }
    }

    public class ByIDObject
    {
        public string license_id {get; set;} = string.Empty;
    }

    public class BySerialObject
    {
        public string serial_number {get; set;} = string.Empty;
    }

    public class BySerialUserObject
    {
        public string user_id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }

    public class ByNameObject
    {
        public string name { get; set; } = string.Empty;
    }

    public class ByPasswordObject
    {
        public string password { get; set; } = string.Empty;
    }

    public class ByUsernameObject
    {
        public string username {get; set;} = string.Empty;
    }

    public class ByUsernameLicenseIDObject
    {
        public string username {get; set;} = string.Empty;
        public string license_id {get; set;} = string.Empty;
    }

    public class ByUsernameSerialObject
    {
        public string username {get; set;} = string.Empty;
        public string serial_number {get; set;} = string.Empty;
    }

    public class ByUserIDLicenseIDObject
    {
        public string user_id {get; set;} = string.Empty;
        public string license_id {get; set;} = string.Empty;
    }

    public class ByUserIDSerialObject
    {
        public string user_id {get; set;} = string.Empty;
        public string serial_number {get; set;} = string.Empty;
    }

    public class ByUserIDObject
    {
        public string user_id { get; set; } = string.Empty;
    }

    public class ByRoleName
    {
        public string role_name { get; set; } = string.Empty;
    }

    public class ConnectionObject
    {
        public string ID {get; set;}
        public string MachineName {get; set;}
        public string UserAgent {get; set;}
        public string UserID {get; set;}
        public string DenID {get; set;}
        public bool Connected {get; set;}
        public ConnectionState State {get; set;}
        public DateTime LastSeen {get; set;}
    }

    public class Connection
    {
        public Oid _id {get; set;} = new Oid();
        public string connection_id {get; set;} = string.Empty;
        public string machine_name {get; set;} = string.Empty;
        public string user_agent {get; set;} = string.Empty;
        public string user_id {get; set;} = string.Empty;
        public string den_id {get; set;} = string.Empty;
        public bool connected {get; set;} = false;
        public ConnectionState state {get; set;} = ConnectionState.OFFLINE;
        public long cow_id {get; set;} = -1;
        public TimeStamp last_seen {get; set;} = new TimeStamp();

        public ConnectionObject ToConnectionObject()
        {
            return new ConnectionObject
            {
                ID = this.connection_id,
                MachineName = this.machine_name,
                UserAgent = this.user_agent,
                UserID = this.user_id,
                DenID = this.den_id,
                Connected = this.connected,
                State = this.state,
                LastSeen = RestAPIUtils.GetDateTime(this.last_seen)
            };
        }
    }

    public class GroupObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string RoleID { get; set; }
    }

    public class Group
    {
        public Oid _id { get; set; } = new Oid();
        public string name { get; set; } = string.Empty;
        public Oid role_id { get; set; } = new Oid();

        public GroupObject ToGroupObject => new GroupObject { ID = _id.oid, Name = name, RoleID = role_id?.oid };
    }

    public class UserGroupObject
    {
        public string UserGroupID { get; set; }
    }

    public class RoleObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public class Role
    {
        public Oid _id { get; set; } = new Oid();
        public string name { get; set; } = string.Empty;

        public RoleObject ToRoleObject => new RoleObject { ID = _id.oid, Name = name };
    }

    public enum ConnectionState
    {
        ONLINE,
        OFFLINE
    }
}