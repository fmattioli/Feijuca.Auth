namespace Feijuca.Auth.Http.Requests;

public record ReplicateRealmRequest(string Tenant, ReplicationConfigurationRequest ReplicationConfigurationRequest);
