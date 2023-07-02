namespace AccountingBot.Models
{
    /// <summary>
    /// User information in the database
    /// </summary>
    public class UserRecord
    {
        /// <summary>
        /// username 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// salt
        /// </summary>
        public string Salt { get; set; }
    }
}
