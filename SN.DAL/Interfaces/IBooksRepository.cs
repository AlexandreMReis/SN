using SN.DAL.Models;
using System;
using System.Collections.Generic;

namespace SN.DAL.Interfaces
{
    public interface IBooksRepository : IDisposable
    {
        /// <summary>
        /// Inserts the book
        /// </summary>
        /// <param name="input">The input</param>
        bool SP_InsertBook(SPInsertBookInput input);

        /// <summary>
        /// Add Reading with rating
        /// </summary>
        /// <param name="bookId">The book id</param>
        /// <param name="memberId">The member id</param>
        /// <param name="rating">The rating</param>
        bool SP_InsertReading(int bookId, int memberId, LikedRating rating);

        /// <summary>
        /// Gets all books
        /// </summary>
        DbQueryResponse<VW_BOOK> GetAllBooks();

        /// <summary>
        /// Gets read books by member id
        /// </summary>
        /// <param name="memberId">The memberId</param>
        /// <param name="onlyRated">The onlyRated filter</param>
        DbQueryResponse<VW_BOOK> GetBooksByMemberId(int memberId, List<LikedRating> ratingFilter = null);

        /// <summary>
        /// Gets books by author name
        /// </summary>
        /// <param name="authorName">The memberId</param>
        DbQueryResponse<VW_BOOK> GetBooksByAuthorName(string authorName);
    }
}
