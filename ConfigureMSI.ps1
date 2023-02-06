# Install the Azure AD module if you don't have it yet. (You need admin on the machine.)
 # Install-Module AzureAD
  
 $tenantID = '3b68c6c1-04d4-4e86-875f-e48fa80b9529'
 $serverApplicationName = 'hello-service'
 $managedIdentityName = 'id-func-msi'
 $appRoleName = 'hello-role'
  
 Connect-AzureAD -TenantId $tenantID
  
 # Look up the web app's managed identity's object ID.
 #$managedIdentity = (Get-AzureADServicePrincipal -Filter "DisplayName eq '$managedIdentityName'")
 #$managedIdentityObjectId = $managedIdentity.ObjectId
  
 # Look up the details about the server app's service principal and app role.
 #$serverServicePrincipal = (Get-AzureADServicePrincipal -Filter "DisplayName eq '$serverApplicationName'")
 #$serverServicePrincipalObjectId = $serverServicePrincipal.ObjectId
 #$appRoleId = ($serverServicePrincipal.AppRoles | Where-Object {$_.Value -eq $appRoleName }).Id
  
 $managedIdentityObjectId = "50b3e21d-76f1-4397-8950-b424f5915ac2"
 $serverServicePrincipalObjectId = "74a0edcb-9a34-4427-9644-b1707faef23a"
 $appRoleId = "88f86e96-4b0c-4ff0-8146-5f8aa2cae4c6"

 # Assign the managed identity access to the app role.
 New-AzureADServiceAppRoleAssignment -ObjectId $managedIdentityObjectId  -Id $appRoleId -PrincipalId $managedIdentityObjectId -ResourceId $serverServicePrincipalObjectId