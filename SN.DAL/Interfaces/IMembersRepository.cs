using SN.DAL.Models;
using System;

namespace SN.DAL.Interfaces
{
    public interface IMembersRepository : IDisposable
    {
        /// <summary>
        /// Creates the member
        /// </summary>
        /// <param name="member">The member</param>
        bool CreateMember(string memberName);

        /// <summary>
        /// Gets all members
        /// </summary>
        DbQueryResponse<MEMBER> GetAllMembers();

        /// <summary>
        /// Add friendship
        /// </summary>
        /// <param name="requester">The member1</param>
        /// <param name="addressed">The member2</param>
        bool SP_InsertFriendship(MEMBER member1, MEMBER member2);

        /// <summary>
        /// Gets members who read book with specified id
        /// </summary>
        /// <param name="bookId">The book id</param>
        DbQueryResponse<MEMBER> GetMembersByBookId(int bookId);

        /// <summary>
        /// Gets friends of member with specified id
        /// </summary>
        /// <param name="memberId">The member id</param>
        DbQueryResponse<MEMBER> GetFriendsByMemberId(int memberId);
    }
}
