package com.japaneseLearning.controller;

import com.japaneseLearning.dto.ApiResponse;
import com.japaneseLearning.entity.Quiz;
import com.japaneseLearning.repository.QuizRepository;
import com.japaneseLearning.repository.QuestionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/quizzes")
@CrossOrigin(origins = "*", maxAge = 3600)
public class QuizController {
    
    private final QuizRepository quizRepository;
    private final QuestionRepository questionRepository;

    public QuizController(QuizRepository quizRepository, QuestionRepository questionRepository) {
        this.quizRepository = quizRepository;
        this.questionRepository = questionRepository;
    }

    @GetMapping
    public ResponseEntity<ApiResponse<List<Quiz>>> getAllQuizzes() {
        List<Quiz> quizzes = quizRepository.findAll();
        return ResponseEntity.ok(ApiResponse.success(quizzes, "Quizzes retrieved successfully"));
    }

    @GetMapping("/{id}")
    public ResponseEntity<ApiResponse<Quiz>> getQuizById(@PathVariable Long id) {
        Quiz quiz = quizRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Quiz not found with id: " + id));
        return ResponseEntity.ok(ApiResponse.success(quiz, "Quiz retrieved successfully"));
    }

    @GetMapping("/course/{courseId}")
    public ResponseEntity<ApiResponse<List<Quiz>>> getQuizzesByCourse(@PathVariable Long courseId) {
        List<Quiz> quizzes = quizRepository.findByCourse_CourseId(courseId);
        return ResponseEntity.ok(ApiResponse.success(quizzes, "Quizzes by course retrieved successfully"));
    }

    @GetMapping("/lesson/{lessonId}")
    public ResponseEntity<ApiResponse<List<Quiz>>> getQuizzesByLesson(@PathVariable Long lessonId) {
        List<Quiz> quizzes = quizRepository.findByLesson_LessonId(lessonId);
        return ResponseEntity.ok(ApiResponse.success(quizzes, "Quizzes by lesson retrieved successfully"));
    }

    @PostMapping
    public ResponseEntity<ApiResponse<Quiz>> createQuiz(@RequestBody Quiz quiz) {
        Quiz saved = quizRepository.save(quiz);
        return ResponseEntity.status(HttpStatus.CREATED)
                .body(ApiResponse.success(saved, "Quiz created successfully"));
    }

    @PutMapping("/{id}")
    public ResponseEntity<ApiResponse<Quiz>> updateQuiz(@PathVariable Long id, @RequestBody Quiz quizDetails) {
        Quiz quiz = quizRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Quiz not found with id: " + id));
        
        quiz.setTitle(quizDetails.getTitle());
        
        Quiz updated = quizRepository.save(quiz);
        return ResponseEntity.ok(ApiResponse.success(updated, "Quiz updated successfully"));
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<ApiResponse<Void>> deleteQuiz(@PathVariable Long id) {
        quizRepository.deleteById(id);
        return ResponseEntity.ok(ApiResponse.success(null, "Quiz deleted successfully"));
    }

    @GetMapping("/{id}/details")
    public ResponseEntity<ApiResponse<Map<String, Object>>> getQuizDetails(@PathVariable Long id) {
        Quiz quiz = quizRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Quiz not found with id: " + id));
        
        Map<String, Object> details = Map.of(
            "quiz", quiz,
            "questionCount", quiz.getQuestions().size(),
            "questions", quiz.getQuestions()
        );
        return ResponseEntity.ok(ApiResponse.success(details, "Quiz details retrieved"));
    }
}
