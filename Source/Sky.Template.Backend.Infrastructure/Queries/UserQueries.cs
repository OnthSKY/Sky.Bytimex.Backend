namespace Sky.Template.Backend.Infrastructure.Queries;

public class UserQueries
{
    #region Get
	 internal const string GetActiveUsers = @"SELECT id, first_name, last_name, email, image_path from $db.users where status='ACTIVE' ";

	internal const string GetActiveUsersByRoleId = @"SELECT u.id, first_name, last_name, email, image_path from $db.users as u
														inner join $db.user_roles as ur on ur.user_id = u.id
														inner join $db.roles as r on r.id=@role_id
														where u.status='ACTIVE' ";

    internal const string GetAllUsersWithRole = @"SELECT 
												u.id, u.first_name, last_name, email, image_path, r.id as role_id, r.name as role_name, r.description as role_description
												from $db.users as u
												inner join $db.user_roles as ur on ur.user_id = u.id
												inner join $db.roles as r on r.id=ur.role_id";


    internal const string GetActiveUsersWithRole = @"
                                SELECT 
                                    u.id,
                                    u.first_name,
                                    u.last_name,
                                    u.email,
                                    u.image_path,
                                    r.id AS role_id,
                                    r.name AS role_name,
                                    r.description AS role_description,
                                    STRING_AGG(p.name, ',') AS permission_names
                                FROM $db.users AS u
                                INNER JOIN $db.user_roles AS ur ON ur.user_id = u.id
                                INNER JOIN $db.roles AS r ON r.id = ur.role_id
                                LEFT JOIN $db.role_permissions AS rp ON rp.role_id = r.id
                                LEFT JOIN $db.permissions AS p ON p.id = rp.permission_id
                                WHERE u.status = 'ACTIVE' /**extra_where**/
                                GROUP BY u.id, u.first_name, u.last_name, u.email, u.image_path, r.id, r.name, r.description ";





    #region Delegation
    internal const string GetActiveDelegationsGivenByUserId = @"select u.fullname  as delegated_user_fullname,u.image_path as delegated_user_image,u.email as delegated_user_email,d.* 
																		from $db.user_delegations as d 
																		inner join $db.users as u on u.id=d.delegated_user_id 
																	where 
																			d.user_id=@user_id 
																			and d.status='ACTIVE' 
																			and end_date > getDate();"; // Verdi�im Vekalet

    internal const string GetReceivedDelegationsByUserIdAndType = @"select u.id as user_to_delegate_userid,u.fullname  as user_to_delegate_fullname,u.image_path as user_to_delegate_photo,u.email as user_to_delegate_email,d.end_date 
															from $db.user_delegations as d 
															inner join $db.users as u on u.id=d.user_id 
														where 
															d.delegated_user_id=@user_id 
															and d.delegation_type=@delegation_type
															and d.status='ACTIVE' 
															and end_date > getDate();"; // Bana Verilen Vekalet
    #endregion


    #endregion

    #region Update

    internal const string UpdateUserImageFromAzureLogin = @"UPDATE $db.users SET image_path = @image_path WHERE id = @user_id; ";

    internal const string UpdateUser = @"
        UPDATE sys.users
        SET first_name = @first_name,
            last_name = @last_name,
            email = @email,
            status = @status,
            updated_at = @updated_at,
            updated_by = @updated_by
        WHERE id = @id AND is_deleted = FALSE
        RETURNING *";

    internal const string SoftDeleteUser = @"
        UPDATE sys.users
        SET is_deleted = TRUE,
            deleted_at = @deleted_at,
            deleted_by = @deleted_by,
            delete_reason = @delete_reason
        WHERE id = @id";


    #endregion
}