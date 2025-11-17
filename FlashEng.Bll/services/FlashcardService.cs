using AutoMapper;
using FlashEng.Bll.Dto;
using FlashEng.Bll.Interfaces;
using FlashEng.Dal.Interfaces;
using FlashEng.Domain.Exceptions;
using FlashEng.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlashEng.Bll.Services
{
    public class FlashcardService : IFlashcardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FlashcardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<FlashcardDto>> GetAllFlashcardsAsync(CancellationToken cancellationToken = default)
        {
            var flashcards = await _unitOfWork.Flashcards.GetAllFlashcardsAsync(cancellationToken);
            return _mapper.Map<List<FlashcardDto>>(flashcards);
        }

        public async Task<List<FlashcardDto>> GetUserFlashcardsAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ValidationException("User ID must be positive");

            var flashcards = await _unitOfWork.Flashcards.GetUserFlashcardsAsync(userId, cancellationToken);
            return _mapper.Map<List<FlashcardDto>>(flashcards);
        }

        public async Task<FlashcardDto?> GetFlashcardByIdAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            if (flashcardId <= 0)
                throw new ValidationException("Flashcard ID must be positive");

            var flashcard = await _unitOfWork.Flashcards.GetFlashcardByIdAsync(flashcardId, cancellationToken);
            return flashcard != null ? _mapper.Map<FlashcardDto>(flashcard) : null;
        }

        public async Task<List<FlashcardDto>> GetFlashcardsByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ValidationException("Category cannot be empty");

            var flashcards = await _unitOfWork.Flashcards.GetFlashcardsByCategoryAsync(category, cancellationToken);
            return _mapper.Map<List<FlashcardDto>>(flashcards);
        }

        public async Task<List<FlashcardDto>> SearchFlashcardsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ValidationException("Search term cannot be empty");

            var flashcards = await _unitOfWork.Flashcards.SearchFlashcardsAsync(searchTerm, cancellationToken);
            return _mapper.Map<List<FlashcardDto>>(flashcards);
        }

        public async Task<int> CreateFlashcardAsync(CreateFlashcardDto createFlashcardDto, CancellationToken cancellationToken = default)
        {
            // Валідація
            if (createFlashcardDto.UserId <= 0)
                throw new ValidationException("User ID must be positive");

            if (string.IsNullOrWhiteSpace(createFlashcardDto.Category))
                throw new ValidationException("Category is required");

            if (string.IsNullOrWhiteSpace(createFlashcardDto.EnglishWord))
                throw new ValidationException("English word is required");

            if (string.IsNullOrWhiteSpace(createFlashcardDto.Translation))
                throw new ValidationException("Translation is required");

            // Перевірка існування користувача
            var user = await _unitOfWork.Users.GetUserByIdAsync(createFlashcardDto.UserId, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", createFlashcardDto.UserId);

            var flashcard = _mapper.Map<Flashcard>(createFlashcardDto);
            flashcard.CreatedAt = DateTime.Now;
            flashcard.UpdatedAt = DateTime.Now;

            return await _unitOfWork.Flashcards.CreateFlashcardAsync(flashcard, cancellationToken);
        }

        public async Task<bool> UpdateFlashcardAsync(int flashcardId, UpdateFlashcardDto updateFlashcardDto, CancellationToken cancellationToken = default)
        {
            if (flashcardId <= 0)
                throw new ValidationException("Flashcard ID must be positive");

            var existingFlashcard = await _unitOfWork.Flashcards.GetFlashcardByIdAsync(flashcardId, cancellationToken);
            if (existingFlashcard == null)
                throw new NotFoundException("Flashcard", flashcardId);

            if (string.IsNullOrWhiteSpace(updateFlashcardDto.Category))
                throw new ValidationException("Category is required");

            if (string.IsNullOrWhiteSpace(updateFlashcardDto.EnglishWord))
                throw new ValidationException("English word is required");

            if (string.IsNullOrWhiteSpace(updateFlashcardDto.Translation))
                throw new ValidationException("Translation is required");

            _mapper.Map(updateFlashcardDto, existingFlashcard);
            existingFlashcard.UpdatedAt = DateTime.Now;

            return await _unitOfWork.Flashcards.UpdateFlashcardAsync(existingFlashcard, cancellationToken);
        }

        public async Task<bool> DeleteFlashcardAsync(int flashcardId, CancellationToken cancellationToken = default)
        {
            if (flashcardId <= 0)
                throw new ValidationException("Flashcard ID must be positive");

            var existingFlashcard = await _unitOfWork.Flashcards.GetFlashcardByIdAsync(flashcardId, cancellationToken);
            if (existingFlashcard == null)
                throw new NotFoundException("Flashcard", flashcardId);

            return await _unitOfWork.Flashcards.DeleteFlashcardAsync(flashcardId, cancellationToken);
        }

        public async Task<List<string>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Flashcards.GetAllCategoriesAsync(cancellationToken);
        }
    }
}
