using Newtonsoft.Json;
using Npgsql;
using SN.DAL.Interfaces;
using SN.DAL.Models;
using SN.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SN.CLI
{
    public class SocialNetwork : ISocialNetwork
    {
        private const int _recommendedCount = 10;
        private readonly IMembersRepository _membersRepository;
        private readonly IBooksRepository _booksRepository;


        public SocialNetwork(IMembersRepository membersRepository, IBooksRepository booksRepository)
        {
            _membersRepository = membersRepository;
            _booksRepository = booksRepository;
        }

        #region Private Methods
        private async Task<bool> CreateMemberAsync(CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                bool output = false;
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {
                    Console.WriteLine("Insira o nome do novo membro.");
                    string authorName = Console.ReadLine();
                    if (string.IsNullOrEmpty(authorName))
                    {
                        Console.WriteLine("Nome inválido.");
                        return false;
                    }

                    output = _membersRepository.CreateMember(authorName);

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = false;
                }
                finally
                {
                    sw.Stop();
                    string message = output == true ? "Membro inserido com sucesso" : "Não foi possivel adicionar o membro.";
                    Console.WriteLine(message);
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<bool> CreateWorkAsync(CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                bool output = false;
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {
                    Console.WriteLine("Insira o título da nova obra.");
                    string workTitle = Console.ReadLine();
                    if (string.IsNullOrEmpty(workTitle))
                    {
                        Console.WriteLine("Nome inválido.");
                        return false;
                    }
                    Console.WriteLine("Insira o nome do autor.");
                    string authorName = Console.ReadLine();
                    if (string.IsNullOrEmpty(authorName))
                    {
                        Console.WriteLine("Id inválido.");
                        return false;
                    }

                    SPInsertBookInput insertBookInput = new SPInsertBookInput
                    {
                        BOOK_TITLE = workTitle,
                        AUTHORS_NAMES = new List<string>() { authorName }
                    };

                    output = _booksRepository.SP_InsertBook(insertBookInput);

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = false;
                }
                finally
                {
                    sw.Stop();
                    string message = output == true ? "Livro adicionado com sucesso" : "Não foi possivel adicionar livro.";
                    Console.WriteLine(message);
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<bool> CreateFriendshipAsync(CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                bool output = false;
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {
                    Console.WriteLine("Insira os ids dos dois membros separados por um espaço. (Exemplo: '1 2')");
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine("Ids inválidos.");
                        return false;
                    }
                    List<string> authorsIds = input.Split(" ").ToList();

                    if (authorsIds == null || authorsIds.Count != 2)
                    {
                        Console.WriteLine("Ids inválidos.");

                        return false;
                    }

                    var allMembers = _membersRepository.GetAllMembers();

                    var member1 = allMembers.Items.FirstOrDefault(m => m.ID.ToString() == authorsIds[0]);
                    var member2 = allMembers.Items.FirstOrDefault(m => m.ID.ToString() == authorsIds[1]);

                    if (member1 == null || member2 == null)
                    {
                        Console.WriteLine($"Não foi possivel concluir ação porque não existem membros com os ids especificados: '{input}'");
                    }

                    output = _membersRepository.SP_InsertFriendship(member1, member2);

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = false;
                }
                finally
                {
                    sw.Stop();
                    string message = output == true ? "Amizade adicionada com sucesso" : "Não foi possivel adicionar amizade";
                    Console.WriteLine(message);
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<bool> ReadAndClassifyWorkAsync(CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                bool output = false;
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {
                    Console.WriteLine("Insira o id do membro que leu o livro, o id do livro e se gostou do livro (1- não gostou nada; 2- gostou pouco; 3- gostou muito) " +
                        "Exemplo: '1 2 3' em que '1' é o id do autor, 2 o id do livro e 3 que gostou muito de ter lido o livro");
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine("Ids inválidos.");
                        return false;
                    }

                    List<string> inputParameters = input.Split(" ").ToList();
                    if (inputParameters == null || inputParameters.Count != 3)
                    {
                        Console.WriteLine("Ids inválidos.");
                        return false;
                    }
                    var member = _membersRepository.GetAllMembers().Items.FirstOrDefault(m => m.ID.ToString() == inputParameters[0]);

                    var book = _booksRepository.GetAllBooks().Items.FirstOrDefault(b => b.ID.ToString() == inputParameters[1]);

                    int rating = -1;
                    if (!Int32.TryParse(inputParameters[2], out rating) && Enum.IsDefined(typeof(LikedRating), rating))
                    {
                        Console.WriteLine($"Erro não foi possivel traduzir parametro de calssificação. Parâmetro foi = '{inputParameters[2]}'");
                        return false;
                    }

                    output = _booksRepository.SP_InsertReading(book.ID, member.ID, (LikedRating)rating);

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = false;
                }
                finally
                {
                    sw.Stop();
                    string message = output == true ? "Obra marcada como lida e classificada." : "Não foi possivel marcar obra como lida.";
                    Console.WriteLine(message);
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<List<VW_BOOK>> GetBooksByMemberId(int memberId, bool onlyRated = false, CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                List<VW_BOOK> output = new List<VW_BOOK>();
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {
                    List<LikedRating> likedFilter = onlyRated ? new List<LikedRating>() { LikedRating.Nothing, LikedRating.Little, LikedRating.ALot } : null;
                    var dbResponse = _booksRepository.GetBooksByMemberId(memberId, likedFilter);
                    if (dbResponse?.Success ?? false)
                    {
                        output = dbResponse.Items;
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = new List<VW_BOOK>();
                }
                finally
                {
                    sw.Stop();
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<List<MEMBER>> GetMembersByBookIdAsync(int bookId, CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                List<MEMBER> output = new List<MEMBER>();
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {

                    var dbResponse = _membersRepository.GetMembersByBookId(bookId);
                    if (dbResponse?.Success ?? false)
                    {
                        output = dbResponse.Items;
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = new List<MEMBER>();
                }
                finally
                {
                    sw.Stop();
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<List<MEMBER>> GetAllMembersAsync(CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                List<MEMBER> output = new List<MEMBER>();
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {

                    var dbResponse = _membersRepository.GetAllMembers();
                    if (dbResponse?.Success ?? false)
                    {
                        output = dbResponse.Items;
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = new List<MEMBER>();
                }
                finally
                {
                    sw.Stop();
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<List<VW_BOOK>> GetAllBooksAsync(CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                List<VW_BOOK> output = new List<VW_BOOK>();
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {
                    var dbResponse = _booksRepository.GetAllBooks();
                    if (dbResponse?.Success ?? false)
                    {
                        output = dbResponse.Items;
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = new List<VW_BOOK>();
                }
                finally
                {
                    sw.Stop();
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<List<MEMBER>> GetFriendsByMemberIdAsync(int memberId, CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                List<MEMBER> output = new List<MEMBER>();
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {

                    var dbResponse = _membersRepository.GetFriendsByMemberId(memberId);
                    if (dbResponse?.Success ?? false)
                    {
                        output = dbResponse.Items;
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = new List<MEMBER>();
                }
                finally
                {
                    sw.Stop();
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<List<MEMBER>> GetFriendsByMemberIdAndBookIdAsync(int memberId, int bookId, CancellationToken ct = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                List<MEMBER> output = new List<MEMBER>();
                string methodName = MethodBase.GetCurrentMethod().GetName();
                var sw = Stopwatch.StartNew();

                try
                {

                    var dbResponse = _membersRepository.GetFriendsByMemberId(memberId);
                    if (dbResponse?.Success ?? false)
                    {
                        output = dbResponse.Items;
                    }

                    dbResponse = _membersRepository.GetMembersByBookId(bookId);
                    if (dbResponse?.Success ?? false)
                    {
                        output = output.Where(friend => dbResponse.Items.Any(bookMember => bookMember.ID == friend.ID)).ToList();
                    };

                    return output;
                }
                catch (Exception ex)
                {
                    LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                    return output = new List<MEMBER>();
                }
                finally
                {
                    sw.Stop();
                    LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
                }
            }, ct).ConfigureAwait(true);
        }

        private async Task<List<VW_BOOK>> RecommendBooksByMemberIdAsync(int memberId, CancellationToken ct = default(CancellationToken))
        {
            List<VW_BOOK> output = new List<VW_BOOK>();
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                var likedFilter = new List<LikedRating>() { LikedRating.Little, LikedRating.ALot };
                var dbResponse = _booksRepository.GetBooksByMemberId(memberId, likedFilter);
                if (dbResponse?.Success ?? false)
                {
                    foreach (VW_BOOK book in dbResponse.Items)
                    {
                        foreach (var authorName in book.AUTHORS)
                        {
                            var newDbResponse = _booksRepository.GetBooksByAuthorName(authorName);
                            var recommendedBooks = newDbResponse.Items.Where(b => !dbResponse.Items.Any(memberBook => memberBook.ID == b.ID)).ToList();
                            output.Concat(recommendedBooks);
                        }
                    }
                }

                if (!output.Any())
                {
                    output = (await this.GetAllBooksAsync(ct)).Take(_recommendedCount).ToList();
                }

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.CLILogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = new List<VW_BOOK>();
            }
            finally
            {
                sw.Stop();
                LogEngine.CLILogger.WriteToLog(LogLevels.Debug, $"BLL.{methodName}(OUT={output}) in {sw.ElapsedMilliseconds}ms");
            }
        }

        #endregion

        public async Task StartAsync(CancellationToken ct = default(CancellationToken))
        {
            Console.WriteLine("Bem-vindo à rede social dos autores");
            while (true)
            {
                Console.WriteLine("\nMenu Principal");
                Console.WriteLine("Escolha uma das opções em baixo:");
                Console.WriteLine("1. Inscrever membro");
                Console.WriteLine("2. Criar obra");
                Console.WriteLine("3. Adicionar amizade");
                Console.WriteLine("4. Marcar obra como lida por um membro e classifica-la");
                Console.WriteLine("5. Listar membros");
                Console.WriteLine("6. Listar Obras");
                Console.WriteLine("7. Recomendar uma leitura a um membro");
                Console.WriteLine("8. Listar obras lidas por um membro");
                Console.WriteLine("9. Listar obras classificadas por um membro");
                Console.WriteLine("10. Listar membros que leram uma obra");
                Console.WriteLine("11. Listar amigos de um membro");
                Console.WriteLine("12. Listar amigos de um membro que leram uma obra");
                Console.WriteLine("Selecione uma opção pelo seu id");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Os dados inseridos não estão corretos. Insira novamente.");
                    continue;
                }
                input = input.Trim();

                string memberId = null;
                int memberIdInt = -1;
                string bookId = null;
                int bookIdInt = -1;
                var books = new List<VW_BOOK>();
                var members = new List<MEMBER>();
                switch (input)
                {
                    case "1":
                        await this.CreateMemberAsync(ct);
                        break;
                    case "2":
                        await this.CreateWorkAsync(ct);
                        break;
                    case "3":
                        await this.CreateFriendshipAsync(ct);
                        break;
                    case "4":
                        await this.ReadAndClassifyWorkAsync(ct);
                        break;
                    case "5":
                        members = await this.GetAllMembersAsync(ct);
                        if (!members.Any())
                        {
                            Console.WriteLine("Não foram encontrados membros.");
                            break;
                        }
                        Console.WriteLine("Foram encontradas os seguintes membros:");
                        foreach (MEMBER member in members)
                        {
                            Console.WriteLine($"Membro(ID={member.ID}; NOME={member.NAME})");
                        }
                        break;
                    case "6":
                        books = await this.GetAllBooksAsync(ct);
                        if (!books.Any())
                        {
                            Console.WriteLine("Não foram encontradas obras.");
                            break;
                        }
                        Console.WriteLine("Foram encontradas as seguintes obras:");
                        foreach (VW_BOOK book in books)
                        {
                            Console.WriteLine($"Obra(ID={book.ID}; TITULO={book.TITLE}; AUTORES=({string.Join(", ", book.AUTHORS.Select(a => $"'{a}'"))})");
                        }
                        break;
                    case "7":
                        Console.WriteLine("Introduza o id do membro.");
                        memberId = Console.ReadLine();
                        memberIdInt = -1;
                        if (!Int32.TryParse(memberId, out memberIdInt))
                        {
                            Console.WriteLine("Id inválido.");
                            break;
                        }
                        books = await this.RecommendBooksByMemberIdAsync(memberIdInt, ct);
                        if (!books.Any())
                        {
                            Console.WriteLine("Não foram encontradas obras.");
                            break;
                        }
                        Console.WriteLine("São recomendadas as seguintes obras:");
                        foreach (VW_BOOK book in books)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(book));
                        }
                        break;
                    case "8":
                        Console.WriteLine("Introduza o id do membro.");
                        memberId = Console.ReadLine();
                        memberIdInt = -1;
                        if (!Int32.TryParse(memberId, out memberIdInt))
                        {
                            Console.WriteLine("Id inválido.");
                            break;
                        }

                        books = await this.GetBooksByMemberId(memberIdInt, false, ct);
                        if (!books.Any())
                        {
                            Console.WriteLine("Não foram encontradas obras.");
                            break;
                        }
                        Console.WriteLine("Foram encontradas as seguintes obras:");
                        foreach (VW_BOOK book in books)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(book));
                        }
                        break;
                    case "9":
                        Console.WriteLine("Introduza o id do membro.");
                        memberId = Console.ReadLine();
                        memberIdInt = -1;
                        if (!Int32.TryParse(memberId, out memberIdInt))
                        {
                            Console.WriteLine("Id inválido.");
                            break;
                        }

                        books = await this.GetBooksByMemberId(memberIdInt, false, ct);
                        if (!books.Any())
                        {
                            Console.WriteLine("Não foram encontradas obras.");
                            break;
                        }
                        Console.WriteLine("Foram encontradas as seguintes obras:");
                        foreach (VW_BOOK book in books)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(book));
                        }
                        break;

                    case "10":
                        Console.WriteLine("Introduza o id da obra.");
                        bookId = Console.ReadLine();
                        bookIdInt = -1;
                        if (!Int32.TryParse(bookId, out bookIdInt))
                        {
                            Console.WriteLine("Id inválido.");
                            break;
                        }
                        members = await this.GetMembersByBookIdAsync(bookIdInt, ct);
                        if (!members.Any())
                        {
                            Console.WriteLine("Não foram encontrados membros.");
                            break;
                        }
                        Console.WriteLine("Foram encontrados os seguintes membros:");
                        foreach (MEMBER member in members)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(member));
                        }
                        break;

                    case "11":
                        Console.WriteLine("Introduza o id do membro.");
                        memberId = Console.ReadLine();
                        memberIdInt = -1;
                        if (!Int32.TryParse(memberId, out memberIdInt))
                        {
                            Console.WriteLine("Id inválido.");
                            break;
                        }
                        members = await this.GetFriendsByMemberIdAsync(memberIdInt, ct);
                        if (!members.Any())
                        {
                            Console.WriteLine("Não foram encontrados membros.");
                            break;
                        }
                        Console.WriteLine($"Membro com id={memberId} tem como amigos os seguintes membros:");
                        foreach (MEMBER member in members)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(member));
                        }
                        break;

                    case "12":
                        Console.WriteLine("Introduza o id do membro.");
                        memberId = Console.ReadLine();
                        memberIdInt = -1;
                        if (!Int32.TryParse(memberId, out memberIdInt))
                        {
                            Console.WriteLine("Id inválido.");
                            break;
                        }
                        Console.WriteLine("Introduza o id da obra.");
                        bookId = Console.ReadLine();
                        bookIdInt = -1;
                        if (!Int32.TryParse(bookId, out bookIdInt))
                        {
                            Console.WriteLine("Id inválido.");
                            break;
                        }

                        members = await this.GetFriendsByMemberIdAndBookIdAsync(memberIdInt, bookIdInt, ct);
                        if (!members.Any())
                        {
                            Console.WriteLine("Não foram encontrados membros.");
                            break;
                        }
                        Console.WriteLine("Foram encontrados os seguintes membros:");
                        foreach (MEMBER member in members)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(member));
                        }
                        break;
                    default:
                        Console.WriteLine($"Valor inserido incorretos. Inseriu '{input}', insira uma das opções: 1,2,3,4,5,6,7.");
                        break;
                }
            }
        }
    }
}
